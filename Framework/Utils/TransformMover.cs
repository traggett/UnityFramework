using UnityEngine;

namespace Framework
{
	using Maths;

	public class TransformMover : MonoBehaviour
	{
		#region Public Data
		public float _targetTolerance = Mathf.Epsilon;
		#endregion

		#region Private Data
		private struct Target
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

		private Target _positionTarget;
		protected bool _positionTargetIsWorldSpace;
		#endregion

		#region Public Interface
		public void SetLocalPosition(Vector2 position, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
		{
			Vector2 currentPosition = this.transform.localPosition;

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

		public void SetWorldPosition(Vector2 position, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
		{
			Vector2 currentPosition = this.transform.position;

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
		}
		#endregion
	}
}