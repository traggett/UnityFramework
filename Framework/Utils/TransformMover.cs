using UnityEngine;

namespace Framework
{
	using Maths;

	public class TransformMover : MonoBehaviour
	{
		#region Public Data
		public float _targetTolerance = 0.001f;
		#endregion

		#region Private Data
		private struct Vector3Target
		{
			public Vector3 _from;
			public Vector3 _target;
			public float _lerp;
			public float _lerpSpeed;
			public InterpolationType _interpolationType;

			public void SetTarget(Vector3 target, float moveTime, Vector3 current, float tolerance, InterpolationType interpolationType)
			{
				if (!IsLerping() || _interpolationType != interpolationType || !MathUtils.Approximately(target, _target, tolerance))
				{
					_lerp = 1f;
					_from = current;
					_interpolationType = interpolationType;
				}

				_lerpSpeed = 1f / moveTime;
				_target = target;
			}

			public void Clear()
			{
				_lerp = 0f;
			}

			public bool IsLerping()
			{
				return _lerp > 0f;
			}

			public Vector3 Update(float deltaTime)
			{
				_lerp -= _lerpSpeed * deltaTime;

				if (_lerp < 0f)
					return _target;

				return MathUtils.Interpolate(_interpolationType, _from, _target, 1f - _lerp);
			}
		}

		private struct QuaternionTarget
		{
			public Quaternion _from;
			public Quaternion _target;
			public float _lerp;
			public float _lerpSpeed;
			public InterpolationType _interpolationType;

			public void SetTarget(Quaternion target, float moveTime, Quaternion current, float tolerance, InterpolationType interpolationType)
			{
				if (!IsLerping() || _interpolationType != interpolationType 
					|| !MathUtils.Approximately(target.eulerAngles.x, _target.eulerAngles.x, tolerance)
					|| !MathUtils.Approximately(target.eulerAngles.y, _target.eulerAngles.y, tolerance)
					|| !MathUtils.Approximately(target.eulerAngles.z, _target.eulerAngles.z, tolerance))
				{
					_lerp = 1f;
					_from = current;
					_interpolationType = interpolationType;
				}

				_lerpSpeed = 1f / moveTime;
				_target = target;
			}

			public void Clear()
			{
				_lerp = 0f;
			}

			public bool IsLerping()
			{
				return _lerp > 0f;
			}

			public Quaternion Update(float deltaTime)
			{
				_lerp -= _lerpSpeed * deltaTime;

				if (_lerp < 0f)
					return _target;

				return MathUtils.Interpolate(_interpolationType, _from, _target, 1f - _lerp);
			}
		}

		private Vector3Target _positionTarget;
		private bool _positionTargetIsWorldSpace;

		private QuaternionTarget _rotationTarget;
		private bool _rotationTargetIsWorldSpace;

		private Vector3Target _scaleTarget;
		#endregion

		#region Public Interface
		public void SetLocalPosition(Vector3 position, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
		{
			Vector3 currentPosition = this.transform.localPosition;

			if (time > 0f)
			{
				if (_positionTargetIsWorldSpace)
					_positionTarget.Clear();

				_positionTarget.SetTarget(position, time, currentPosition, _targetTolerance, interpolationType);
				_positionTargetIsWorldSpace = false;
			}
			else
			{
				_positionTarget.Clear();
				this.transform.localPosition = position;
			}
		}

		public void SetWorldPosition(Vector3 position, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
		{
			Vector3 currentPosition = this.transform.position;

			if (time > 0f)
			{
				if (!_positionTargetIsWorldSpace)
					_positionTarget.Clear();

				_positionTarget.SetTarget(position, time, currentPosition, _targetTolerance, interpolationType);
				_positionTargetIsWorldSpace = true;
			}
			else
			{
				_positionTarget.Clear();
				this.transform.localPosition = position;
			}
		}

		public void SetLocalRotation(Quaternion rotation, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
		{
			Quaternion currentRotation = this.transform.localRotation;

			if (time > 0f)
			{
				if (_rotationTargetIsWorldSpace)
					_rotationTarget.Clear();

				_rotationTarget.SetTarget(rotation, time, currentRotation, _targetTolerance, interpolationType);
				_rotationTargetIsWorldSpace = false;
			}
			else
			{
				_rotationTarget.Clear();
				this.transform.localRotation = rotation;
			}
		}

		public void SetWorldRotation(Quaternion rotation, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
		{
			Quaternion currentRotation = this.transform.rotation;

			if (time > 0f)
			{
				if (!_rotationTargetIsWorldSpace)
					_rotationTarget.Clear();

				_rotationTarget.SetTarget(rotation, time, currentRotation, _targetTolerance, interpolationType);
				_rotationTargetIsWorldSpace = true;
			}
			else
			{
				_rotationTarget.Clear();
				this.transform.rotation = rotation;
			}
		}
		
		public void SetLocalScale(Vector3 scale, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
		{
			Vector3 currentScale = this.transform.localScale;

			if (time > 0f)
			{
				_scaleTarget.SetTarget(scale, time, currentScale, _targetTolerance, interpolationType);
			}
			else
			{
				_scaleTarget.Clear();
				this.transform.localScale = scale;
			}
		}
		#endregion

		#region Unity Messages
		private void Update()
		{
			float deltaTime = Time.deltaTime;

			Transform transform = this.transform;

			if (_positionTarget.IsLerping())
			{
				if (_positionTargetIsWorldSpace)
				{
					transform.position = _positionTarget.Update(deltaTime);
				}
				else
				{
					transform.localPosition = _positionTarget.Update(deltaTime);
				}
			}

			if (_rotationTarget.IsLerping())
			{
				if (_rotationTargetIsWorldSpace)
				{
					transform.rotation = _rotationTarget.Update(deltaTime);
				}
				else
				{
					transform.localRotation = _rotationTarget.Update(deltaTime);
				}
			}

			if (_scaleTarget.IsLerping())
			{
				transform.localScale = _scaleTarget.Update(deltaTime);
			}
		}
		#endregion
	}
}