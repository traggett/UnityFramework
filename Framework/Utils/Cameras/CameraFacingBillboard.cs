using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
	public Camera m_Camera;

	void LateUpdate()
	{
		if (m_Camera != null)
			transform.LookAt(m_Camera.transform.position, m_Camera.transform.rotation * Vector3.up);
	}
}