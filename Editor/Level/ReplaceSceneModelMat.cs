/*
 * 关卡白盒阶段，替换场景中选择物体的材质
 */

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace yuxuetian
{
    public class ReplaceSceneModelMat : EditorWindow
    {
        public Material targetMaterial;

        [MenuItem("ArtTools/Level/ReplaceSceneMaterial", false, 301)]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<ReplaceSceneModelMat>("替换场景模型材质");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("用来让场景中的模型材质保持一致,便于调色,主要使用在白盒阶段。", MessageType.None);
            GUILayout.Space(10);
            targetMaterial = EditorGUILayout.ObjectField(targetMaterial, typeof(Material), true) as Material;
            if (GUILayout.Button("开始执行"))
            {
                ReplaceSelectedModelsMaterials();
            }
        }

        private void ReplaceSelectedModelsMaterials()
        {
            // 获取当前选择的所有GameObject  
            GameObject[] selectObjects = Selection.gameObjects;

            // 检查是否有选中模型  
            if (selectObjects.Length > 0)
            {
                foreach (GameObject obj in selectObjects)
                {
                    Debug.Log($"正在处理: {obj.name}");

                    // 获取GameObject上的所有Renderer组件  
                    Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);

                    foreach (Renderer renderer in renderers)
                    {
                        // 检查Renderer是否有材质  
                        if (renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0)
                        {
                            // 替换所有材质  
                            Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
                            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                            {
                                // 如果targetMaterial为null，可以选择保留原材质或设置为默认材质  
                                // 这里我们直接设置为targetMaterial（如果targetMaterial存在）  
                                newMaterials[i] = targetMaterial != null ? targetMaterial : renderer.sharedMaterials[i];
                            }

                            renderer.materials = newMaterials; // 注意：这里使用的是.materials而不是.sharedMaterials来修改实例  
                        }
                    }
                }

                if (targetMaterial == null)
                {
                    Debug.LogWarning("未指定材质，未对选中模型进行材质替换！");
                }
            }
            else
            {
                Debug.LogWarning("未选中任何物体！");
            }
        }
    }
}
