using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MelonLoader;
using Unity.Mathematics;
using UnityEngine;
using BulletMenuVR;
using Random = System.Random;

namespace CustomNPCs
{
    public static class BuildInfo
    {
        public const string Name = "CustomNPCs"; // Name of the Mod.  (MUST BE SET)
        public const string Author = "notnotnotswipez"; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.2"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class CustomNPCs : MelonMod
    {
        public static Dictionary<string, Vector3> fixedBoneRotations = new Dictionary<string, Vector3>();
        public static Dictionary<string, Vector3> npcStartingBoneRotations = new Dictionary<string, Vector3>();
        public static List<string> boneNames = new List<string>();
        public static List<EnemyRoot> customNpcs = new List<EnemyRoot>();

        public static List<string> possibleNames = new List<string>();
        private static String selectedNPC = null;
        private static bool randomizeWaves = false;
        private static bool shouldWorkOnInfiniteWaves = false;

        private static EnemyRoot latestGeneratorEnemy;

        public override void OnApplicationStart()
        {
            Directory.CreateDirectory(MelonUtils.UserDataDirectory + "/CustomNPCs");

            string[] files = Directory.GetFiles(MelonUtils.UserDataDirectory+"/CustomNPCs", "*.npc");
            
            VrMenuPageBuilder selectionBuilder = VrMenuPageBuilder.Builder();

            selectionBuilder.AddButton(new VrMenuButton("None", () =>
                {
                    selectedNPC = null;
                }, Color.red
                ));
            
            foreach (String otherFiles in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(otherFiles);
                possibleNames.Add(fileName);
                
                selectionBuilder.AddButton(new VrMenuButton(fileName, () =>
                    {
                        selectedNPC = fileName;
                    }, Color.black
                    ));
            }

            VrMenuPage selectionPage = selectionBuilder.Build();
            VrMenuPageBuilder randomizerBuilder = VrMenuPageBuilder.Builder();
            randomizerBuilder.AddButton(new VrMenuButton("Disable Infinite Waves Custom NPCs", () =>
                {
                    shouldWorkOnInfiniteWaves = false;
                }, Color.red
            ));
            
            randomizerBuilder.AddButton(new VrMenuButton("Enable Infinite Waves Custom NPCs", () =>
                {
                    shouldWorkOnInfiniteWaves = true;
                }, Color.green
            ));
            
            randomizerBuilder.AddButton(new VrMenuButton("Disable Randomize NPCs in Infinity Waves", () =>
                {
                    randomizeWaves = false;
                }, Color.red
            ));
            
            randomizerBuilder.AddButton(new VrMenuButton("Randomize NPCs in Infinity Waves", () =>
                {
                    shouldWorkOnInfiniteWaves = true;
                    randomizeWaves = true;
            }, Color.green
                ));

            VrMenuPage randomizerPage = randomizerBuilder.Build();
            VrMenuPageBuilder mainPageBuilder = VrMenuPageBuilder.Builder();

            mainPageBuilder.AddButton(new VrMenuButton("NPC Select", () =>
            {
                selectionPage.Open();
            }));
            
            mainPageBuilder.AddButton(new VrMenuButton("Infinity Waves", () =>
            {
                randomizerPage.Open();
            }));

            VrMenuPage mainPage = mainPageBuilder.Build();
            
            VrMenu.RegisterMainButton(new VrMenuButton("Custom NPCs", () =>
            {
                mainPage.Open();
            }));
            
            boneNames.Add("SkullModel");
            boneNames.Add("PhysicalJawTarget");
            boneNames.Add("Thigh");
            boneNames.Add("UpperArm");
            boneNames.Add("ChestEntrails");
            
            fixedBoneRotations.Add("pelvis", new Vector3(0.0f, 90.0f, 0.0f));
            fixedBoneRotations.Add("thigh_l", new Vector3(1.6f, 90.1f, 175.9f));
            fixedBoneRotations.Add("calf_l", new Vector3(16.0f, 89.0f, 178.4f));
            fixedBoneRotations.Add("foot_l", new Vector3(295.9f, 90.1f, 166.8f));
            fixedBoneRotations.Add("thigh_r", new Vector3(8.1f, 89.5f, 188.5f));
            fixedBoneRotations.Add("calf_r", new Vector3(11.3f, 90.0f, 188.8f));
            fixedBoneRotations.Add("foot_r", new Vector3(309.7f, 86.4f, 202.3f));
            fixedBoneRotations.Add("spine_01", new Vector3(352.7f, 90.0f, 0.0f));
            fixedBoneRotations.Add("spine_02", new Vector3(352.7f, 90.0f, 0.0f));
            fixedBoneRotations.Add("spine_03", new Vector3(352.7f, 90.0f, 0.0f));
            fixedBoneRotations.Add("clavicle_l", new Vector3(73.4f, 149.8f, 151.3f));
            fixedBoneRotations.Add("upperarm_l", new Vector3(18.3f, 179.7f, 180.0f));
            fixedBoneRotations.Add("lowerarm_l", new Vector3(5.7f, 176.6f, 167.2f));
            fixedBoneRotations.Add("hand_l", new Vector3(1.2f, 200.1f, 174.9f));
            fixedBoneRotations.Add("neck_01", new Vector3(0.0f, 90.0f, 0.0f));
            fixedBoneRotations.Add("head", new Vector3(0.0f, 90.0f, 0.0f));
            fixedBoneRotations.Add("clavicle_r", new Vector3(71.0f, 24.8f, 204.5f));
            fixedBoneRotations.Add("upperarm_r", new Vector3(21.0f, 0.9f, 181.6f));
            fixedBoneRotations.Add("lowerarm_r", new Vector3(6.1f, 10.2f, 196.3f));
            fixedBoneRotations.Add("hand_r", new Vector3(6.6f, 5.9f, 191.9f));
            
            npcStartingBoneRotations.Add("root", new Vector3(270f, 89.99962f, 0f));
            npcStartingBoneRotations.Add("pelvis", new Vector3(359.9358f, 184.4637f, 270.4815f));
            npcStartingBoneRotations.Add("thigh_l", new Vector3(8.15668f, 173.441f, 267.7521f));
            npcStartingBoneRotations.Add("thigh_twist_01_l", new Vector3(8.327586f, 178.9338f, 268.5964f));
            npcStartingBoneRotations.Add("calf_l", new Vector3(2.719432f, 183.0643f, 282.8113f));
            npcStartingBoneRotations.Add("calf_twist_01_l", new Vector3(3.008848f, 182.8015f, 283.6706f));
            npcStartingBoneRotations.Add("foot_l", new Vector3(359.7301f, 181.9635f, 270.6106f));
            npcStartingBoneRotations.Add("ball_l", new Vector3(359.7344f, 181.9548f, 2.492124f));
            npcStartingBoneRotations.Add("thigh_r", new Vector3(351.4557f, 192.2657f, 91.11668f));
            npcStartingBoneRotations.Add("thigh_twist_01_r", new Vector3(351.6059f, 186.7689f, 91.98359f));
            npcStartingBoneRotations.Add("calf_r", new Vector3(356.262f, 187.3356f, 102.3812f));
            npcStartingBoneRotations.Add("calf_twist_01_r", new Vector3(355.9748f, 187.6007f, 103.2356f));
            npcStartingBoneRotations.Add("foot_r", new Vector3(359.536f, 195.8096f, 92.75099f));
            npcStartingBoneRotations.Add("ball_r", new Vector3(359.5535f, 195.8265f, 184.1445f));
            npcStartingBoneRotations.Add("spine_01", new Vector3(358.8558f, 184.3812f, 277.4615f));
            npcStartingBoneRotations.Add("spine_02", new Vector3(359.4415f, 182.4554f, 264.3822f));
            npcStartingBoneRotations.Add("spine_03", new Vector3(359.4415f, 182.4554f, 261.6028f));
            npcStartingBoneRotations.Add("clavicle_l", new Vector3(278.2905f, 343.8468f, 86.11373f));
            npcStartingBoneRotations.Add("upperarm_l", new Vector3(347.6549f, 27.72342f, 77.48131f));
            npcStartingBoneRotations.Add("upperarm_twist_01_l", new Vector3(347.6551f, 27.72342f, 77.48112f));
            npcStartingBoneRotations.Add("lowerarm_l", new Vector3(346.4409f, 29.04144f, 110.6302f));
            npcStartingBoneRotations.Add("hand_l", new Vector3(7.182298f, 315.5613f, 103.6744f));
            npcStartingBoneRotations.Add("middle_01_l", new Vector3(10.31653f, 320.7238f, 65.56126f));
            npcStartingBoneRotations.Add("middle_02_l", new Vector3(16.27994f, 316.0125f, 29.20985f));
            npcStartingBoneRotations.Add("middle_03_l", new Vector3(20.54132f, 314.0967f, 10.07737f));
            npcStartingBoneRotations.Add("ring_01_l", new Vector3(358.6666f, 305.47f, 57.75902f));
            npcStartingBoneRotations.Add("ring_02_l", new Vector3(1.367205f, 308.5297f, 23.6674f));
            npcStartingBoneRotations.Add("ring_03_l", new Vector3(4.774271f, 306.1891f, 2.096951f));
            npcStartingBoneRotations.Add("index_01_l", new Vector3(13.63145f, 334.6859f, 76.20506f));
            npcStartingBoneRotations.Add("index_02_l", new Vector3(25.68698f, 332.3141f, 48.74823f));
            npcStartingBoneRotations.Add("index_03_l", new Vector3(36.61045f, 321.5544f, 29.22751f));
            npcStartingBoneRotations.Add("thumb_01_l", new Vector3(12.32823f, 47.89695f, 125.0221f));
            npcStartingBoneRotations.Add("thumb_02_l", new Vector3(15.40295f, 42.94625f, 106.9237f));
            npcStartingBoneRotations.Add("thumb_03_l", new Vector3(15.0866f, 38.85083f, 111.0976f));
            npcStartingBoneRotations.Add("pinky_01_l", new Vector3(352.5666f, 301.3074f, 61.48701f));
            npcStartingBoneRotations.Add("pinky_02_l", new Vector3(355.0344f, 307.4337f, 26.44539f));
            npcStartingBoneRotations.Add("pinky_03_l", new Vector3(359.3047f, 306.8917f, 351.2636f));
            npcStartingBoneRotations.Add("lowerarm_twist_01_l", new Vector3(346.441f, 29.04144f, 110.6302f));
            npcStartingBoneRotations.Add("clavicle_r", new Vector3(84.47646f, 24.3166f, 270.244f));
            npcStartingBoneRotations.Add("upperarm_r", new Vector3(16.52847f, 332.5979f, 263.9202f));
            npcStartingBoneRotations.Add("upperarm_twist_01_r", new Vector3(17.58203f, 353.449f, 270.0985f));
            npcStartingBoneRotations.Add("lowerarm_r", new Vector3(17.40118f, 334.7668f, 286.0838f));
            npcStartingBoneRotations.Add("hand_r", new Vector3(346.7275f, 48.77039f, 287.8882f));
            npcStartingBoneRotations.Add("AimingParent", new Vector3(75.27374f, 121.6568f, 60.88676f));
            npcStartingBoneRotations.Add("TargetAiming", new Vector3(75.27375f, 121.6568f, 60.8868f));
            npcStartingBoneRotations.Add("middle_01_r", new Vector3(347.1903f, 42.58516f, 259.5388f));
            npcStartingBoneRotations.Add("middle_02_r", new Vector3(341.7425f, 44.34082f, 233.1754f));
            npcStartingBoneRotations.Add("middle_03_r", new Vector3(338.5513f, 42.7038f, 224.6481f));
            npcStartingBoneRotations.Add("ring_01_r", new Vector3(354.8238f, 59.2237f, 247.6354f));
            npcStartingBoneRotations.Add("ring_02_r", new Vector3(352.6611f, 55.76963f, 220.7423f));
            npcStartingBoneRotations.Add("ring_03_r", new Vector3(348.3617f, 56.98845f, 205.9559f));
            npcStartingBoneRotations.Add("index_01_r", new Vector3(344.2755f, 28.68169f, 265.8052f));
            npcStartingBoneRotations.Add("index_02_r", new Vector3(333.7045f, 27.76102f, 243.1819f));
            npcStartingBoneRotations.Add("index_03_r", new Vector3(321.9613f, 32.84692f, 230.223f));
            npcStartingBoneRotations.Add("thumb_01_r", new Vector3(352.4722f, 316.5494f, 315.8899f));
            npcStartingBoneRotations.Add("thumb_02_r", new Vector3(348.7072f, 320.8224f, 295.3593f));
            npcStartingBoneRotations.Add("thumb_03_r", new Vector3(348.497f, 324.8737f, 297.1597f));
            npcStartingBoneRotations.Add("pinky_01_r", new Vector3(0.05095493f, 63.77827f, 250.8168f));
            npcStartingBoneRotations.Add("pinky_02_r", new Vector3(358.2986f, 57.77447f, 223.274f));
            npcStartingBoneRotations.Add("pinky_03_r", new Vector3(353.4954f, 57.44502f, 194.9016f));
            npcStartingBoneRotations.Add("lowerarm_twist_01_r", new Vector3(13.23932f, 348.0994f, 289.6236f));
            npcStartingBoneRotations.Add("CC_Base_NeckTwist01", new Vector3(19.22621f, 92.65044f, 0.590753f));
            npcStartingBoneRotations.Add("neck_01", new Vector3(0.6767672f, 182.5597f, 290.9768f));
            npcStartingBoneRotations.Add("head", new Vector3(359.9742f, 178.1008f, 276.9633f));
            npcStartingBoneRotations.Add("cap", new Vector3(359.9742f, 178.1008f, 276.9633f));
            npcStartingBoneRotations.Add("LeftEyeGougeTransform", new Vector3(353.4496f, 247.9669f, 272.3676f));
            npcStartingBoneRotations.Add("RightEyeGougeTransform", new Vector3(353.4674f, 288.2403f, 87.58361f));
            npcStartingBoneRotations.Add("LeftEye", new Vector3(3.159316f, 89.38143f, 359.9429f));
            npcStartingBoneRotations.Add("CC_Base_L_Eye", new Vector3(273.1599f, 90.41734f, 358.9657f));
            npcStartingBoneRotations.Add("RightEye", new Vector3(3.160189f, 88.72524f, 359.939f));
            npcStartingBoneRotations.Add("CC_Base_R_Eye", new Vector3(273.1608f, 89.83205f, 358.8949f));
            npcStartingBoneRotations.Add("PhysicalJawTarget", new Vector3(0.0363474f, 358.1045f, 174.1334f));
            npcStartingBoneRotations.Add("CC_Base_JawRoot", new Vector3(0.0363474f, 358.1045f, 174.1334f));
            npcStartingBoneRotations.Add("CC_Base_Teeth02", new Vector3(359.9739f, 178.0944f, 14.08693f));
            npcStartingBoneRotations.Add("CC_Base_Tongue01", new Vector3(0.03601579f, 358.1031f, 173.5262f));
            npcStartingBoneRotations.Add("CC_Base_Tongue02", new Vector3(0.03504266f, 358.1014f, 159.8548f));
            npcStartingBoneRotations.Add("CC_Base_Tongue03", new Vector3(0.03522929f, 358.1014f, 159.8549f));
            npcStartingBoneRotations.Add("JawModel", new Vector3(14.49283f, 88.10767f, 0.0261949f));
            npcStartingBoneRotations.Add("CC_Base_NeckTwist02", new Vector3(28.48101f, 88.11406f, 0.02947497f));
            npcStartingBoneRotations.Add("CC_Base_Head", new Vector3(5.869103f, 88.10693f, 0.02814491f));
            npcStartingBoneRotations.Add("CC_Base_FacialBone", new Vector3(0.02800858f, 358.1041f, 262.1033f));
            npcStartingBoneRotations.Add("CC_Base_UpperJaw", new Vector3(0.02755938f, 358.0992f, 172.1027f));
            npcStartingBoneRotations.Add("CC_Base_Teeth01", new Vector3(359.9706f, 178.103f, 7.896055f));
            npcStartingBoneRotations.Add("HeadForwardDirection", new Vector3(6.963346f, 88.10399f, 0.0260033f));
            npcStartingBoneRotations.Add("LookingRaycastPosition", new Vector3(6.963308f, 88.10394f, 0.02607598f));
            npcStartingBoneRotations.Add("SkullModel", new Vector3(7.652921f, 88.10434f, 0.02590025f));
            npcStartingBoneRotations.Add("HeadDeform", new Vector3(43.03727f, 268.0769f, 359.9647f));
            //npcStartingBoneRotations.Add("spine_03", new Vector3(359.4416f, 182.456f, 261.6029f));
            npcStartingBoneRotations.Add("MeshDeformationPoint", new Vector3(0.2374725f, 272.453f, 359.4415f));
            npcStartingBoneRotations.Add("StandartAimTransform", new Vector3(0f, 89.99962f, 0f));
            npcStartingBoneRotations.Add("ChestAimHelper", new Vector3(0f, 89.99962f, 0f));
            npcStartingBoneRotations.Add("ChestEntrails", new Vector3(0f, 89.99962f, -3.562222E-13f));
            npcStartingBoneRotations.Add("Entrails", new Vector3(-3.975693E-16f, 89.99962f, -8.060978E-09f));
        }

        public static void FixRotateEnemy(EnemyRoot enemyRoot)
        {
            foreach (Transform otherChild in enemyRoot.transform.Find("[BODY]").Find("AnimatorBones").GetComponentsInChildren<Transform>())
            {
                if (npcStartingBoneRotations.ContainsKey(otherChild.gameObject.name))
                {
                    otherChild.rotation = Quaternion.Euler(npcStartingBoneRotations[otherChild.gameObject.name]);
                }
            }
        }

        public static IEnumerator WaitForCustomizeGenerator(List<GameObject> enemies)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            EnemyRoot enemyRoot = enemies[enemies.Count - 1].GetComponent<EnemyRoot>();
            MakeCustomNPC(enemyRoot, false);
        }
        
        public static IEnumerator WaitForCustomizeRandomizer(List<HealthContainer> enemies)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            EnemyRoot enemyRoot = enemies[enemies.Count - 1].GetComponentInParent<EnemyRoot>();
            if (!shouldWorkOnInfiniteWaves)
            {
                yield break;
            }
            MakeCustomNPC(enemyRoot, true);
        }
        public static void MakeCustomNPC(EnemyRoot enemyRoot, bool shouldRandomize)
        {
            if (selectedNPC == null)
            {
                return;
            }

            if (customNpcs.Contains(enemyRoot))
            {
                enemyRoot.SkinChanger.ClearSkins();
                return;
            }

            Dictionary<GameObject, Vector3> originalRotations = new Dictionary<GameObject, Vector3>();

            originalRotations.Add(enemyRoot.gameObject, enemyRoot.transform.eulerAngles);
            
            foreach (Transform otherChild in enemyRoot.transform.Find("[BODY]").Find("AnimatorBones").GetComponentsInChildren<Transform>())
            {
                originalRotations.Add(otherChild.gameObject, otherChild.eulerAngles);
            }
            
            customNpcs.Add(enemyRoot);
            
            enemyRoot.transform.rotation = quaternion.Euler(0, 90f, 0);

            FixRotateEnemy(enemyRoot);

            string selection = selectedNPC;
            if (shouldRandomize)
            {
                Random random = new Random();
                selection = possibleNames.GetRange(random.Next(0, possibleNames.Count), 1)[0];
            }

            AssetBundle assetBundle = AssetBundle.LoadFromFile(MelonUtils.UserDataDirectory+"/CustomNPCs/"+selection+".npc");
            GameObject loadedModel = assetBundle.LoadAsset<GameObject>(assetBundle.GetAllAssetNames()[0]);
            assetBundle.Unload(false);
            
            GameObject spawnedNPC = GameObject.Instantiate(loadedModel);
            var transform = enemyRoot.transform;
            spawnedNPC.transform.position = transform.position + new Vector3(0.1f, 0.04f, 0);
            spawnedNPC.transform.rotation = transform.rotation;
            spawnedNPC.transform.localScale = transform.localScale;
            FixBones(spawnedNPC.transform);
            List<GameObject> customBones = new List<GameObject>();
            
            foreach (Transform realBones in spawnedNPC.transform.Find("actualNPC").Find("NPC").Find("bones").GetComponentsInChildren<Transform>())
            {
                customBones.Add(realBones.gameObject);
            }
 
            spawnedNPC.transform.Find("actualNPC").Find("NPC").Find("Mesh").parent = enemyRoot.transform;
            enemyRoot.SkinChanger.ClearSkins();
            LinkBones(customBones, enemyRoot);

            foreach (GameObject returnedTransforms in originalRotations.Keys)
            {
                returnedTransforms.transform.rotation = Quaternion.Euler(originalRotations[returnedTransforms]);
            }
        }

        public static bool IsBone(GameObject gameObject)
        {
            if (boneNames.Contains(gameObject.name))
            {
                return true;
            }

            return false;
        }

        public static void LinkBones(List<GameObject> customBones, EnemyRoot root)
        {
            foreach (GameObject bone in customBones)
            {
                GameObject child = bone;
                foreach (Transform rootChild in root.transform.Find("[BODY]").Find("AnimatorBones").gameObject.GetComponentsInChildren<Transform>()) {
                    if (IsBone(rootChild.gameObject))
                    {
                        rootChild.gameObject.SetActive(false);
                    }
                    
                    if (rootChild.gameObject.name.Equals(child.name) && !customBones.Contains(rootChild.gameObject))
                    {
                        child.transform.parent = rootChild;
                    }
                }
            }
        }

        public static void FixBones(Transform parent)
        {
            int childCount = parent.childCount;
            for (int  i = 0; i < childCount; i++)
            {
                GameObject child = parent.GetChild(i).gameObject;
                if (fixedBoneRotations.ContainsKey(child.name))
                {
                    child.transform.rotation = Quaternion.Euler(fixedBoneRotations[child.name]);
                }

                if (child.transform.childCount > 0)
                {
                    FixBones(child.transform);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(InfinityWaveSpawner), "TrySpawn")]
    class InfinityWavePatch
    {
        public static void Postfix(InfinityWaveSpawner __instance, ref List<HealthContainer> ____aliveEnemies,
            bool __result)
        {
            if (__result)
            {
                MelonCoroutines.Start(CustomNPCs.WaitForCustomizeRandomizer(____aliveEnemies));
            }
        }
    }
    
    [HarmonyPatch(typeof(EnemySpawnerFromGenerator), nameof(EnemySpawnerFromGenerator.Spawn),
        new Type[] { typeof(EnemyData) })]
    class EnemySpawnPatch
    {
        public static void Postfix(EnemySpawnerFromGenerator __instance, EnemyData enemyData,
            ref List<GameObject> ___spawnedObjects)
        {
            MelonCoroutines.Start(CustomNPCs.WaitForCustomizeGenerator(___spawnedObjects));
        }
    }
}
