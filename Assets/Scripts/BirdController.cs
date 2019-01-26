using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BirdController : MonoBehaviour {

	bool animating = false;
	bool grounded = false;
	float speed = 1;
	NestItem chosenBit;

	// Animator parameter string keys
	const string stoppedBool = "Stopped";
	const string groundedBool = "Grounded";
	const string pickUpTrigger = "PickUp";
	const float pickUpAnimationTime = 0.2f;


	[System.NonSerialized]
	public static BirdController activeController;

	[SerializeField]
	float airSpeed = 5;

	[SerializeField]
	float groundSpeed = 2;

	[SerializeField]
	Transform beak;

	[SerializeField]
	Animator anim;

	[SerializeField]
	LandscapeConstants LandscapeConstants;

	[SerializeField]
	float minPickupDistance = 0.2f;

	void Awake() {
		activeController = this;
	}


	public void InputMove(Vector2 direction) {
		if (animating) {
			return;
		}
		Move(direction);
	}

	public void Move(Vector2 direction) {
		if (direction.x == 0) {
			anim.SetBool(stoppedBool, true);
			if (direction.y == 0) {
				return;
			}
		} else {
			anim.SetBool(stoppedBool, false);
		}

		Vector3 move = (Vector3)direction * Time.deltaTime;
		Vector3 newPosition = transform.position + move * speed;

		if (newPosition.y <= LandscapeConstants.GroundThreshhold) {
			speed = groundSpeed;
			grounded = true;
			anim.SetBool(groundedBool, true);
			newPosition.y = LandscapeConstants.GroundThreshhold;
		} else {
			speed = airSpeed;
			grounded = false;
			anim.SetBool(groundedBool, false);
		}

		transform.position = newPosition;
	}

	public void Interact() {
		if (null != chosenBit) {
			Drop();
		} else {
			var distance = NestItem.ActiveItems.Min(item => (transform.position - item.transform.position).sqrMagnitude);
			if (distance <= minPickupDistance) {
				var cruft = NestItem.ActiveItems.FirstOrDefault(item => (transform.position - item.transform.position).sqrMagnitude == distance);
				Pickup(cruft);
			}
		}
	}

	private void Drop() {
		chosenBit.isHeld = false;
		chosenBit.transform.SetParent(null);
		StartCoroutine(chosenBit.Fall());
		chosenBit = null;
	}

	private void Pickup(NestItem cruft) {
		chosenBit = cruft;
		chosenBit.isHeld = true;
		chosenBit.transform.SetParent(transform);
		chosenBit.transform.localPosition = beak.localPosition;
	}

	IEnumerator PickUpRoutine(NestItem cruft) {
		if (grounded) {
			animating = true;
			anim.SetTrigger(pickUpTrigger);
			yield return new WaitForSeconds(pickUpAnimationTime);
			animating = false;
		}
		Pickup(cruft);
	}

}