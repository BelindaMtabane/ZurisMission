#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-shot setup for the new Zuri character.
/// Tools > Zuri > 1 - Report Files     : logs what is in Assets/ZuriCharacterAnim (changes nothing)
/// Tools > Zuri > 2 - Setup Character  : humanoid rigs, animator controller, swaps character under Player,
///                                       attaches the Bucket to the right hand, disables the old "Female".
/// </summary>
public static class ZuriCharacterSetup
{
    const string Folder = "Assets/ZuriCharacterAnim";
    const string ControllerPath = Folder + "/ZuriAnimator.controller";
    const string NewCharacterName = "Zuri";

    // Leave empty to auto-detect, or set the character file name, e.g. "Zuri.fbx"
    const string CharacterFileOverride = "";

    static readonly string[] LoopKeywords = { "run", "idle", "walk", "jog", "sprint", "carry", "breath" };
    static readonly string[] OneShotKeywords = { "jump", "slide", "fall", "hit", "death", "die", "roll", "trip", "stumble" };

    // ---------------------------------------------------------------- helpers

    static List<string> ModelPaths() =>
        AssetDatabase.FindAssets("t:Model", new[] { Folder })
            .Select(AssetDatabase.GUIDToAssetPath).Distinct().OrderBy(p => p).ToList();

    static List<AnimationClip> ClipsIn(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__")).ToList();

    static bool HasSkin(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return go && go.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0;
    }

    static int SkinVerts(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!go) return 0;
        return go.GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .Where(s => s.sharedMesh).Sum(s => s.sharedMesh.vertexCount);
    }

    static bool IsLoop(string clipName)
    {
        var n = clipName.ToLower();
        if (OneShotKeywords.Any(n.Contains)) return false;
        return LoopKeywords.Any(n.Contains);
    }

    static string TriggerName(string clipName)
    {
        var s = new string(clipName.Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrEmpty(s) ? "Play" : char.ToUpper(s[0]) + s.Substring(1);
    }

    static bool SkinBounds(GameObject go, out Bounds b)
    {
        b = default;
        var rs = go.GetComponentsInChildren<SkinnedMeshRenderer>(false);
        if (rs.Length == 0) return false;
        b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b.size.y > 0.01f;
    }

    // ---------------------------------------------------------------- 1. report

    [MenuItem("Tools/Zuri/1 - Report Files")]
    public static void Report()
    {
        var sb = new StringBuilder("[ZuriSetup] REPORT for " + Folder + "\n");
        var models = ModelPaths();
        foreach (var p in models)
        {
            var imp = AssetImporter.GetAtPath(p) as ModelImporter;
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            int smr = go ? go.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length : 0;
            var clips = ClipsIn(p);
            sb.AppendLine($"MODEL {p} | rig={imp?.animationType} | skinnedMeshes={smr} | clips={clips.Count}: " +
                          string.Join(", ", clips.Select(c => c.name + (c.isLooping ? "(loop)" : ""))));
        }
        foreach (var o in AssetDatabase.FindAssets("", new[] { Folder })
                     .Select(AssetDatabase.GUIDToAssetPath)
                     .Where(p => !AssetDatabase.IsValidFolder(p)).Except(models).Distinct())
            sb.AppendLine("OTHER " + o);
        Debug.Log(sb.ToString());
    }

    // ---------------------------------------------------------------- 2. setup

    [MenuItem("Tools/Zuri/2 - Setup Character")]
    public static void Setup()
    {
        var paths = ModelPaths();
        if (paths.Count == 0) { Debug.LogError("[ZuriSetup] No models found in " + Folder); return; }

        // --- Humanoid rigs + clip settings
        foreach (var p in paths)
        {
            var imp = (ModelImporter)AssetImporter.GetAtPath(p);
            imp.animationType = ModelImporterAnimationType.Human;
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            imp.SaveAndReimport();

            var clips = imp.clipAnimations;
            if (clips == null || clips.Length == 0) clips = imp.defaultClipAnimations;
            if (clips.Length == 0) continue;

            string fileName = Path.GetFileNameWithoutExtension(p);
            for (int i = 0; i < clips.Length; i++)
            {
                var c = clips[i];
                // Mixamo clips are all called "mixamo.com" – rename them after the file
                if (clips.Length == 1) c.name = fileName;
                else if (c.name.ToLower().Contains("mixamo")) c.name = fileName + "_" + i;

                c.loopTime = IsLoop(c.name);
                // Keep the character in place – the game moves the Player, not the animation
                c.lockRootRotation = true;   c.keepOriginalOrientation = true;
                c.lockRootHeightY = true;    c.keepOriginalPositionY = true;
                c.lockRootPositionXZ = true; c.keepOriginalPositionXZ = true;
            }
            imp.clipAnimations = clips;
            imp.SaveAndReimport();
        }

        // --- Pick the character model
        string charPath = null;
        if (!string.IsNullOrEmpty(CharacterFileOverride))
            charPath = paths.FirstOrDefault(p => p.EndsWith(CharacterFileOverride));
        if (charPath == null)
            charPath = paths.FirstOrDefault(p =>
            {
                var n = Path.GetFileNameWithoutExtension(p).ToLower();
                return (n.Contains("tpose") || n.Contains("t-pose") || n.Contains("t pose") ||
                        n.Contains("character") || n.Contains("model")) && HasSkin(p);
            });
        if (charPath == null)
            charPath = paths.Where(HasSkin).OrderBy(p => ClipsIn(p).Count)
                .ThenByDescending(SkinVerts).FirstOrDefault();
        if (charPath == null) { Debug.LogError("[ZuriSetup] No skinned character model found."); return; }

        // --- Animator controller
        var allClips = paths.Where(p => p != charPath).SelectMany(ClipsIn).GroupBy(c => c.name).Select(g => g.First()).ToList();
        if (allClips.Count == 0) { Debug.LogError("[ZuriSetup] No animation clips found."); return; }

        AssetDatabase.DeleteAsset(ControllerPath);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        var sm = ctrl.layers[0].stateMachine;

        var def = allClips.FirstOrDefault(c => c.name.ToLower().Contains("run"))
                  ?? allClips.FirstOrDefault(c => c.name.ToLower().Contains("idle"))
                  ?? allClips[0];

        var defState = sm.AddState(def.name, new Vector3(300, 0, 0));
        defState.motion = def;
        sm.defaultState = defState;
        string defTrig = TriggerName(def.name);
        ctrl.AddParameter(defTrig, AnimatorControllerParameterType.Trigger);
        var toDef = sm.AddAnyStateTransition(defState);
        toDef.AddCondition(AnimatorConditionMode.If, 0, defTrig);
        toDef.duration = 0.15f; toDef.canTransitionToSelf = false;

        int row = 1;
        var triggerList = new List<string> { defTrig };
        foreach (var c in allClips.Where(c => c != def))
        {
            var st = sm.AddState(c.name, new Vector3(550, 70 * row++, 0));
            st.motion = c;
            string trig = TriggerName(c.name);
            if (ctrl.parameters.Any(x => x.name == trig)) trig += row;
            ctrl.AddParameter(trig, AnimatorControllerParameterType.Trigger);
            triggerList.Add(trig);

            var tr = sm.AddAnyStateTransition(st);
            tr.AddCondition(AnimatorConditionMode.If, 0, trig);
            tr.duration = 0.1f; tr.canTransitionToSelf = false;

            if (!c.isLooping) // one-shots (jump, slide...) return to the default state
            {
                var back = st.AddTransition(defState);
                back.hasExitTime = true; back.exitTime = 0.9f; back.duration = 0.15f;
            }
        }
        AssetDatabase.SaveAssets();

        // --- Scene swap
        var player = GameObject.Find("Player");
        var old = player ? player.transform.Find("Female") : null;
        if (!player || !old) { Debug.LogError("[ZuriSetup] Could not find Player/Female in the open scene."); return; }

        var charAsset = AssetDatabase.LoadAssetAtPath<GameObject>(charPath);
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(charAsset, player.transform);
        Undo.RegisterCreatedObjectUndo(inst, "Add Zuri");
        inst.name = NewCharacterName;
        inst.transform.localPosition = old.localPosition;
        inst.transform.localRotation = old.localRotation;
        inst.transform.localScale = Vector3.one;

        // match height and feet to the old character
        if (SkinBounds(old.gameObject, out var oldB) && SkinBounds(inst, out var newB))
        {
            inst.transform.localScale *= oldB.size.y / newB.size.y;
            SkinBounds(inst, out newB);
            inst.transform.position += Vector3.up * (oldB.min.y - newB.min.y);
        }

        var anim = inst.GetComponent<Animator>();
        if (anim == null) anim = inst.AddComponent<Animator>();
        if (anim.avatar == null)
            anim.avatar = AssetDatabase.LoadAllAssetsAtPath(charPath).OfType<Avatar>().FirstOrDefault();
        anim.runtimeAnimatorController = ctrl;
        anim.applyRootMotion = false;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // --- Bucket to right hand
        var bucket = old.Find("Bucket");
        var hand = anim.isHuman ? anim.GetBoneTransform(HumanBodyBones.RightHand) : null;
        string bucketMsg;
        if (bucket && hand)
        {
            Undo.SetTransformParent(bucket, hand, "Bucket to hand");
            Undo.RecordObject(bucket, "Place bucket");
            float drop = 0.1f;
            var rs = bucket.GetComponentsInChildren<Renderer>();
            if (rs.Length > 0)
            {
                var bb = rs[0].bounds; foreach (var r in rs) bb.Encapsulate(r.bounds);
                drop = bb.extents.y * 0.8f;
            }
            bucket.position = hand.position + Vector3.down * drop;
            bucketMsg = "Bucket parented to " + hand.name;
        }
        else bucketMsg = "WARNING: bucket or right-hand bone not found – bucket not moved";

        Undo.RecordObject(old.gameObject, "Disable old Female");
        old.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Selection.activeGameObject = inst;

        Debug.Log("[ZuriSetup] DONE\n" +
                  "Character: " + charPath + "\n" +
                  "Controller: " + ControllerPath + " (default state: " + def.name + ")\n" +
                  "Clips: " + string.Join(", ", allClips.Select(c => c.name + (c.isLooping ? "(loop)" : ""))) + "\n" +
                  "Triggers: " + string.Join(", ", triggerList) + "\n" +
                  bucketMsg + "\nOld 'Female' disabled (not deleted).");
    }

    // ---------------------------------------------------------------- 3. fixes

    static string SafeName(string n)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) n = n.Replace(c, '_');
        return n.Replace(':', '_');
    }

    [MenuItem("Tools/Zuri/3 - Fix Textures, Feet and Jerking")]
    public static void Fix()
    {
        var paths = ModelPaths();
        string charPath = !string.IsNullOrEmpty(CharacterFileOverride)
            ? paths.FirstOrDefault(p => p.EndsWith(CharacterFileOverride))
            : paths.Where(HasSkin).OrderByDescending(SkinVerts).FirstOrDefault();
        if (charPath == null) { Debug.LogError("[ZuriSetup] Character model not found."); return; }

        var log = new StringBuilder("[ZuriSetup] FIX\n");
        string dir = Path.GetDirectoryName(charPath).Replace('\\', '/');
        string texDir = dir + "/Textures";
        string matDir = dir + "/Materials";
        if (!AssetDatabase.IsValidFolder(texDir)) AssetDatabase.CreateFolder(dir, "Textures");
        if (!AssetDatabase.IsValidFolder(matDir)) AssetDatabase.CreateFolder(dir, "Materials");

        // --- 1. Textures + materials
        var charImp = (ModelImporter)AssetImporter.GetAtPath(charPath);
        bool texOk = charImp.ExtractTextures(texDir);
        AssetDatabase.Refresh();
        int texCount = AssetDatabase.FindAssets("t:Texture2D", new[] { texDir }).Length;
        log.AppendLine($"Textures extracted: {texOk} ({texCount} in {texDir})");

        charImp = (ModelImporter)AssetImporter.GetAtPath(charPath);
        charImp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        charImp.materialLocation = ModelImporterMaterialLocation.InPrefab;
        charImp.materialSearch = ModelImporterMaterialSearch.RecursiveUp;
        charImp.animationCompression = ModelImporterAnimationCompression.Off;
        charImp.importAnimation = true;
        charImp.SaveAndReimport();

        int matCount = 0;
        foreach (var m in AssetDatabase.LoadAllAssetsAtPath(charPath).OfType<Material>().ToList())
        {
            string mp = AssetDatabase.GenerateUniqueAssetPath(matDir + "/" + SafeName(m.name) + ".mat");
            string err = AssetDatabase.ExtractAsset(m, mp);
            if (string.IsNullOrEmpty(err)) { AssetDatabase.WriteImportSettingsIfDirty(charPath); matCount++; }
            else log.AppendLine("  material '" + m.name + "' not extracted: " + err);
        }
        AssetDatabase.ImportAsset(charPath, ImportAssetOptions.ForceUpdate);
        log.AppendLine($"Materials extracted: {matCount}");
        var textures = AssetDatabase.FindAssets("t:Texture2D", new[] { texDir })
            .Select(g => AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(t => t != null)
            .ToList();
        foreach (var g in AssetDatabase.FindAssets("t:Material", new[] { matDir }))
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
            UpgradeMaterialForUrp(mat);
            var tex = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap")
                    : mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
            if (tex == null)
            {
                tex = FindBestTexture(mat.name, textures);
                if (tex != null && mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                    EditorUtility.SetDirty(mat);
                }
            }
            log.AppendLine($"  {mat.name} | shader={mat.shader.name} | texture={(tex ? tex.name : "NONE")}");
        }
        if (texCount == 0)
            log.AppendLine("  NOTE: zuri.fbx had no embedded textures – they may need to be re-downloaded or added separately.");

        // --- 2. Feet: every animation uses Zuri's own avatar
        var charAvatar = AssetDatabase.LoadAllAssetsAtPath(charPath).OfType<Avatar>().FirstOrDefault();
        if (charAvatar == null || !charAvatar.isHuman)
            log.AppendLine("WARNING: Zuri's avatar is missing or not humanoid – feet fix skipped.");

        // --- 3. Smooth loops
        foreach (var p in paths.Where(p => p != charPath))
        {
            var imp = (ModelImporter)AssetImporter.GetAtPath(p);
            imp.animationType = ModelImporterAnimationType.Human;
            imp.animationCompression = ModelImporterAnimationCompression.Off;
            imp.resampleCurves = true;
            if (charAvatar != null && charAvatar.isHuman)
            {
                imp.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                imp.sourceAvatar = charAvatar;
            }
            var clips = imp.clipAnimations;
            if (clips == null || clips.Length == 0) clips = imp.defaultClipAnimations;
            foreach (var c in clips)
            {
                c.lockRootRotation = true;
                c.keepOriginalOrientation = true;
                c.lockRootHeightY = true;
                c.keepOriginalPositionY = true;
                c.lockRootPositionXZ = true;
                c.keepOriginalPositionXZ = true;
                // Preserve the retargeted clip's body height. Deriving it from the
                // feet makes this Mixamo skeleton fold its lower legs under the body.
                c.heightFromFeet = false;
                if (c.loopTime)
                {
                    c.loopPose = true;
                    c.cycleOffset = 0f;
                }
            }
            imp.clipAnimations = clips;
            imp.SaveAndReimport();
            log.AppendLine("Animation fixed: " + Path.GetFileName(p));
        }

        // --- Keep Foot IK off: it over-corrects this retargeted Mixamo skeleton.
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (ctrl != null)
        {
            foreach (var s in ctrl.layers[0].stateMachine.states) s.state.iKOnFeet = false;
            var layers = ctrl.layers;
            layers[0].iKPass = false;
            ctrl.layers = layers;
            EditorUtility.SetDirty(ctrl);
            log.AppendLine("Foot IK disabled on all states");
        }
        AssetDatabase.SaveAssets();
        Debug.Log(log.ToString());
    }

    static void UpgradeMaterialForUrp(Material mat)
    {
        if (mat == null) return;

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null || mat.shader == urpLit) return;

        Texture albedo = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap")
            : mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex")
            : null;
        Color color = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
            : mat.HasProperty("_Color") ? mat.GetColor("_Color")
            : Color.white;

        mat.shader = urpLit;
        if (mat.HasProperty("_BaseMap") && albedo != null) mat.SetTexture("_BaseMap", albedo);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.25f);
        EditorUtility.SetDirty(mat);
    }

    static Texture2D FindBestTexture(string materialName, List<Texture2D> textures)
    {
        if (textures == null || textures.Count == 0) return null;
        if (textures.Count == 1) return textures[0];

        string key = NormalizedAssetName(materialName);
        Texture2D best = textures.FirstOrDefault(t =>
        {
            string textureKey = NormalizedAssetName(t.name);
            return textureKey.Contains(key) || key.Contains(textureKey);
        });
        if (best != null) return best;

        string[] tokens = { "body", "cloth", "hair", "eye", "lash", "shoe", "sneaker", "sock", "skin" };
        foreach (string token in tokens)
        {
            if (!key.Contains(token)) continue;
            best = textures.FirstOrDefault(t => NormalizedAssetName(t.name).Contains(token));
            if (best != null) return best;
        }

        return null;
    }

    static string NormalizedAssetName(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray())
            .Replace("material", "")
            .Replace("mat", "")
            .Replace("basecolor", "")
            .Replace("albedo", "")
            .Replace("diffuse", "");
    }
}

[InitializeOnLoad]
static class ZuriCharacterAutoRepair
{
    const string SessionKey = "ZuriCharacterAutoRepair_2026_09_28_v2";

    static ZuriCharacterAutoRepair()
    {
        EditorApplication.delayCall += TryRun;
    }

    static void TryRun()
    {
        if (SessionState.GetBool(SessionKey, false)) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            return;
        }

        SessionState.SetBool(SessionKey, true);
        ZuriCharacterSetup.Fix();
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) return;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.delayCall += TryRun;
    }
}
#endif
