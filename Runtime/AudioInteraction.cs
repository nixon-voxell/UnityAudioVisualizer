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

namespace Voxell.Audio
{
  public partial class AudioCore
  {
    [Header("Audio Visualizer  Interaction")]
    [Tooltip("Velocity of audio visualizer when there is no interaction")]
    public Vector2 idleVelocity = new Vector2(1.0f, -1.0f);

    [Tooltip("Minimum turbulence for VFX graph when there is no interaction or extra force")]
    public float idleNoiseIntensity = 0.1f;
    public float maxNoiseIntensity = 0.3f;

    [Tooltip("Sensitivity of the audio visualizer on mouse drag")]
    public float rotationMultiplier = 500;

    [Range(0.8f, 0.99f), Tooltip("A number that multiplies the velocity of the audio visualizer on each update")]
    public float velocityDamping = 0.9f;

    [Tooltip("Multiplier of noise intensity before sending it to the VFX graph")]
    public float intensityCoefficient = 0.1f;

    private Vector2 rotationVelocity;

    private void InitAgentInteraction() => rotationVelocity = Vector2.zero;

    private void UpdateAgentInteraction()
    {
      if (Input.GetMouseButton(0)) OnMouseDrag();

      if (Vector3.Dot(transform.up, Vector3.up) >= 0)
        transform.Rotate(Camera.main.transform.up, -Vector3.Dot(rotationVelocity, Camera.main.transform.right), Space.World);
      else
        transform.Rotate(Camera.main.transform.up, -Vector3.Dot(rotationVelocity, Camera.main.transform.right), Space.World);
      
      transform.Rotate(Camera.main.transform.right, Vector3.Dot(rotationVelocity, Camera.main.transform.up), Space.World);
      rotationVelocity *= velocityDamping;
      rotationVelocity += idleVelocity*Time.deltaTime;

      if (rotationVelocity.magnitude <= EPSILON) rotationVelocity = Vector2.zero;

      float intensity = Mathf.Clamp(rotationVelocity.magnitude * intensityCoefficient, idleNoiseIntensity, maxNoiseIntensity);
      audioVFX.SetFloat(VFXPropertyId.float_noiseIntensity, intensity);
    }

    /// <summary>
    /// Calcualte mouse drag force and apply rotational force accordingly
    /// </summary>
    private void OnMouseDrag()
    {
      float rotationX = Input.GetAxis("Mouse X")*rotationMultiplier*Mathf.Deg2Rad;
      float rotationY = Input.GetAxis("Mouse Y")*rotationMultiplier*Mathf.Deg2Rad;

      rotationVelocity = new Vector2(rotationX, rotationY);
    }
  }
}