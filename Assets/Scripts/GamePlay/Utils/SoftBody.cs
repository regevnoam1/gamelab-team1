using UnityEngine;
using UnityEngine.U2D;

namespace GamePlay.Utils
{
    public class SoftBody : MonoBehaviour
    {
        #region Fields
        [SerializeField] SpriteShapeController spriteShapeController;
        [SerializeField] Transform[] controlPoints;
        private const float SplineOffset = 0.5f;

        #endregion

    
        #region MonoBehaviour Callbacks

        private void Awake()
        {
            UpdateVertices();
        }
    
        private void Update()
        {
            UpdateVertices();
        }

        #endregion

    
        #region Private Methods
    
        private void UpdateVertices()
        {
            for (int i = 0; i < controlPoints.Length - 1; i++)
            {
                Vector2 position = controlPoints[i].localPosition;
            
                Vector2 directionToCenter = (Vector2.zero - position).normalized;
            
                float centerColliderRadius = controlPoints[i].GetComponent<CircleCollider2D>().radius;
            
                try
                {
                    spriteShapeController.spline.SetPosition(i, position - directionToCenter * centerColliderRadius);
                }
                catch
                {
                    Debug.Log("Error: spline points are too close to each other");
                    spriteShapeController.spline.SetPosition(i, position - directionToCenter * (centerColliderRadius + SplineOffset));
                }
            
                Vector2 leftTangent = spriteShapeController.spline.GetLeftTangent(i);
            
                Vector2 newRightTangent = Vector2.Perpendicular(directionToCenter) * leftTangent.magnitude;
                Vector2 newLeftTangent = -newRightTangent;
            
                spriteShapeController.spline.SetRightTangent(i, newLeftTangent);
                spriteShapeController.spline.SetLeftTangent(i, newRightTangent);
            }
        }
    
        #endregion
    
    
    }
}
