/*
This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software Foundation,
Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.

The Original Code is Copyright (C) 2020 Voxell Technologies.
All rights reserved.
*/

using UnityEngine;

namespace SmartAssistant.Audio.Visualizer
{
  public partial class AudioVisualizer
  {
    #region Agent Interaction
    [Header("Agent Interaction")]
    public float rotationMultiplier = 500;
    public Vector2 rotationVelocity;
    [Range(0.8f, 0.99f)]
    public float velocityDamping = 0.9f;
    public float intensityCoefficient = 0.1f;

    public float intervalTime = 0.01f;
    private float timePassed;
    #endregion

    #region Editor Stuffs
    [HideInInspector]
    public bool showAgentInteraction,
    showRotation,
    showMouse;
    #endregion

    void InitAgentInteraction()
    {
      rotationVelocity = Vector2.zero;
      timePassed = 0;
    }

    void UpdateAgentInteraction()
    {
      timePassed += Time.deltaTime;

      if (timePassed >= intervalTime)
      {
        timePassed = 0.0f;
        #region Rotation
        if (Input.GetMouseButton(0)) OnMouseDrag();

        if (Vector3.Dot(transform.up, Vector3.up) >= 0)
        {
          transform.Rotate(Camera.main.transform.up, -Vector3.Dot(rotationVelocity, Camera.main.transform.right), Space.World);
        } else
        {
          transform.Rotate(Camera.main.transform.up, -Vector3.Dot(rotationVelocity, Camera.main.transform.right), Space.World);
        }
        
        transform.Rotate(Camera.main.transform.right, Vector3.Dot(rotationVelocity, Camera.main.transform.up), Space.World);
        rotationVelocity *= velocityDamping;

        if (rotationVelocity.magnitude <= epsilon) rotationVelocity = Vector2.zero;

        float intensity = Mathf.Clamp(rotationVelocity.magnitude * intensityCoefficient, 0, 1);
        audioVFX.SetFloat(noiseIntensity, intensity);
        #endregion
      }
    }

    void OnMouseDrag()
    {
      float rotationX = Input.GetAxis("Mouse X")*rotationMultiplier*Mathf.Deg2Rad;
      float rotationY = Input.GetAxis("Mouse Y")*rotationMultiplier*Mathf.Deg2Rad;

      rotationVelocity = new Vector2(rotationX, rotationY);
    }
  }
}