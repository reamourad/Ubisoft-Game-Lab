  using UnityEngine;

public class Visualization : MonoBehaviour
{
            // Debug visualization for BoxCast
            public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float distance, Color color)
            {
                // Draw the box at start position
                DrawBox(origin, halfExtents, orientation, color);
    
                // Draw the box at end position
                Vector3 endPosition = origin + direction * distance;
                DrawBox(endPosition, halfExtents, orientation, color);
    
                // Draw lines connecting the boxes
                Vector3[] startPoints = GetBoxCorners(origin, halfExtents, orientation);
                Vector3[] endPoints = GetBoxCorners(endPosition, halfExtents, orientation);
    
                for (int i = 0; i < 8; i++)
                {
                    Debug.DrawLine(startPoints[i], endPoints[i], color, 0.1f);
                }
            }
            
            // Draw a box using Debug.DrawLine
            static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
            {
                Vector3[] corners = GetBoxCorners(origin, halfExtents, orientation);
                
                // Draw bottom face
                Debug.DrawLine(corners[0], corners[1], color, 0.1f);
                Debug.DrawLine(corners[1], corners[2], color, 0.1f);
                Debug.DrawLine(corners[2], corners[3], color, 0.1f);
                Debug.DrawLine(corners[3], corners[0], color, 0.1f);
                
                // Draw top face
                Debug.DrawLine(corners[4], corners[5], color, 0.1f);
                Debug.DrawLine(corners[5], corners[6], color, 0.1f);
                Debug.DrawLine(corners[6], corners[7], color, 0.1f);
                Debug.DrawLine(corners[7], corners[4], color, 0.1f);
                
                // Draw connecting lines
                Debug.DrawLine(corners[0], corners[4], color, 0.1f);
                Debug.DrawLine(corners[1], corners[5], color, 0.1f);
                Debug.DrawLine(corners[2], corners[6], color, 0.1f);
                Debug.DrawLine(corners[3], corners[7], color, 0.1f);
            }

            // Get the eight corners of a box
            static Vector3[] GetBoxCorners(Vector3 origin, Vector3 halfExtents, Quaternion orientation)
            {
                Vector3[] corners = new Vector3[8];
                
                // Bottom face (counter-clockwise)
                corners[0] = origin + orientation * new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
                corners[1] = origin + orientation * new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);
                corners[2] = origin + orientation * new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z);
                corners[3] = origin + orientation * new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z);
                
                // Top face (counter-clockwise)
                corners[4] = origin + orientation * new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
                corners[5] = origin + orientation * new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
                corners[6] = origin + orientation * new Vector3(halfExtents.x, halfExtents.y, halfExtents.z);
                corners[7] = origin + orientation * new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z);
                
                return corners;
            }
}
