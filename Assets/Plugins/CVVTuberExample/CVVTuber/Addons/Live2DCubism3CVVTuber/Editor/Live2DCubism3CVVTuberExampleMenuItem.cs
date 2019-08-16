using UnityEngine;
using UnityEditor;
using Live2D.Cubism.Core;
using Live2D.Cubism.Rendering;

namespace CVVTuber.Live2DCubism3
{
    public class Live2DCubism3CVVTuberExampleMenuItem : MonoBehaviour
    {
    
        [MenuItem ("Tools/CVVTuberExample/Setup Live2DCubism3CVVTuberExample_Koharu", false, 1)]
        public static void SetLive2DCubism3CVVTuberSettings_Koharu ()
        {
            GameObject koharu = GameObject.Find ("Koharu");
            if (koharu != null) {

                CubismModel live2DCubism3Model = koharu.GetComponent<CubismModel> ();

                Animator animator = koharu.GetComponent<Animator>();


                CubismRenderController cubisumRenderController = koharu.GetComponent<CubismRenderController>();
                Undo.RecordObject(cubisumRenderController, "Set CubismSortingMode.BackToFrontOrder to cubisumRenderController.SortingMode");
                cubisumRenderController.SortingMode = CubismSortingMode.BackToFrontOrder;
                EditorUtility.SetDirty(cubisumRenderController);
                foreach (var renderer in cubisumRenderController.Renderers)
                {
                    EditorUtility.SetDirty(renderer);
                    // HACK Get mesh renderer directly.
                    EditorUtility.SetDirty(renderer.GetComponent<MeshRenderer>());
                }


                Undo.RecordObject(animator, "Set AnimatorControlle to animator.runtimeAnimatorController");
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animation/Koharu_Animation");
                EditorUtility.SetDirty(animator);


                Live2DCubism3HeadRotationController headRotationController = FindObjectOfType<Live2DCubism3HeadRotationController> ();
                if (headRotationController != null) {
                    Undo.RecordObject (headRotationController, "Set live2DCubism3Model to headRotationController.target");
                    headRotationController.target = live2DCubism3Model;

                    EditorUtility.SetDirty(headRotationController);
                }

                Live2DCubism3FaceAnimationController faceAnimationController = FindObjectOfType<Live2DCubism3FaceAnimationController> ();
                if (faceAnimationController != null) {
                    Undo.RecordObject (faceAnimationController, "Set live2DCubism3Model to faceAnimationController.live2DCubism3Model");
                    faceAnimationController.live2DCubism3Model = live2DCubism3Model;

                    EditorUtility.SetDirty(faceAnimationController);
                }

                Live2DCubism3KeyInputExpressionController keyInputExpressionController = FindObjectOfType<Live2DCubism3KeyInputExpressionController>();
                if (keyInputExpressionController != null)
                {
                    Undo.RecordObject(keyInputExpressionController, "Set Animator to keyInputExpressionController.target");
                    keyInputExpressionController.target = animator;

                    EditorUtility.SetDirty(keyInputExpressionController);
                }

            } else {
                Debug.LogError ("There is no \"Live2DCubism3Model_Koharu\" prefab in the scene. Please add \"Live2DCubism3Model_Koharu\" prefab to the scene.");
            }
        }        
    }
}