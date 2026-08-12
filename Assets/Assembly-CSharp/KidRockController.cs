using UnityEngine;

[RequireComponent(typeof(Animator))]
public class KidRockController : CharacterAnimController
{
	[SerializeField]
	private OWRigidbody _rockBody;

	[SerializeField]
	private Collider _rockCollider;

	[SerializeField]
	private OWRenderer _proxyRock;

	private float _nextThrowTime;

	private bool _throwingRock;

	private void Start()
	{
		Physics.IgnoreCollision(_rockCollider, _dialogueTree.GetComponent<Collider>());
		_rockBody.gameObject.SetActive(value: false);
		_proxyRock.SetActivation(active: false);
		_nextThrowTime = Time.time + 1f;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.OnSectorOccupantsUpdated();
		if (!base.enabled && _rockBody.gameObject.activeSelf)
		{
			_rockBody.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (!_throwingRock && !_dialogueTree.InConversation() && Time.time > _nextThrowTime)
		{
			StartRockThrow();
		}
	}

	private void StartRockThrow()
	{
		_throwingRock = true;
		_animator.SetTrigger("Arkose_RockThrow");
		_dialogueTree.GetInteractVolume().DisableInteraction();
		if (_rockBody.gameObject.activeSelf)
		{
			_rockBody.gameObject.SetActive(value: false);
		}
	}

	private void Arkose_PickupRock()
	{
		_proxyRock.SetActivation(active: true);
	}

	private void Arkose_ReleaseRock()
	{
		if (base.enabled)
		{
			_proxyRock.SetActivation(active: false);
			_throwingRock = false;
			_nextThrowTime = Time.time + Random.Range(4f, 8f);
			_dialogueTree.GetInteractVolume().EnableInteraction();
			_rockBody.gameObject.SetActive(value: true);
			_rockBody.SetPosition(_proxyRock.transform.position);
			_rockBody.SetRotation(_proxyRock.transform.rotation);
			_rockBody.SetVelocity(_rockBody.GetOrigParentBody().GetPointVelocity(_proxyRock.transform.position) + base.transform.forward * 10f + base.transform.up * 4f);
		}
	}

	protected override void OnEndConversation()
	{
		base.OnEndConversation();
		_nextThrowTime = Time.time + Random.Range(4f, 8f);
	}
}
