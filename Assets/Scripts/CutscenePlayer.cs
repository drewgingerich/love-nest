using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutscenePlayer : MonoBehaviour
{
	[SerializeField]
	bool skipCutscenes;

    [SerializeField]
    BirdController player;

	[SerializeField]
	NestItem fallingLeaf;

    [SerializeField]
    HotBird bird;
    [SerializeField]
    Animator birdAnimator;

    [SerializeField]
    HotBird hotBird;
    [SerializeField]
    Animator hotBirdAnimator;

    [SerializeField]
    Transform branchSitPosition;

		[SerializeField]
		new AudioSource audio;
		[SerializeField]
		AudioClip goodNoise, badNoise;

	[SerializeField]
	Image transitionImage;

	const float THOUGHT_BUBBLE_APPEARANCE_TIME = 0.2f;

	const string TALKING = "isTalking";
	const string THINKING = "isThinking";

	const string BIRD_TALK_HAPPY = "Pleased";
	const string BIRD_TALK_ANGRY = "Angered";
	const string BIRD_SING = "Explain";
	const string BIRD_DANCE = "Dance";

	const string THOUGHT_LOVE = "Heart";
	const string THOUGHT_SCRIBBLE = "Scribbles";
	const string THOUGHT_MUSIC = "Music";

  void Start() {
		StartCoroutine(CutsceneRoutine(OpeningCutsceneRoutine, true));
  }

	public void PlaySampleSongCutscene() {
		StartCoroutine(CutsceneRoutine(SampleSongCutsceneRoutine));
	}

	public void PlayGoodItemCutscene() {
		StartCoroutine(CutsceneRoutine(GoodItemCutsceneRoutine));
	}

	public void PlayBadItemCutscene(NestItem item) {
		StartCoroutine(CutsceneRoutine(BadItemCutsceneRoutine));
	}

    public void PlayEndingCutscene() {
		StartCoroutine(CutsceneRoutine(EndingCutsceneRoutine));
    }

	IEnumerator CutsceneRoutine(System.Func<IEnumerator> cutscene, bool rootPlayer = false) {
		if (skipCutscenes) {
			yield break;
		}
		hotBird.EnterCutsceneMode();
		if (rootPlayer) {
			bird.EnterCutsceneMode();
			yield return StartCoroutine(FlyToBranch());
		}
		yield return StartCoroutine(cutscene());
		hotBird.ExitCutsceneMode();
		if (rootPlayer) {
			bird.ExitCutsceneMode();
			ReleasePlayer();
		}
	}

	IEnumerator InteractionRoutine(HotBird bird, string birdState, string thoughtState) {
		bird.thoughtsAnim.SetBool(THINKING, true);
		bird.thoughtsAnim.SetTrigger(thoughtState);
		yield return new WaitForSeconds(THOUGHT_BUBBLE_APPEARANCE_TIME);

		if (birdState == BIRD_SING || birdState == BIRD_DANCE) {
			bird.backgroundMusic.volume = 0;
			bird.song.volume = 1;
		}

		bird.anim.SetBool(TALKING, true);
		bird.anim.SetTrigger(birdState);
		if (birdState == BIRD_SING || birdState == BIRD_DANCE) {
			yield return new WaitForSeconds(5f);
		} else {
			yield return new WaitForSeconds(1.5f);
		}

		bird.thoughtsAnim.SetBool(THINKING, false);
		bird.anim.SetBool(TALKING, false);

		if (birdState == BIRD_SING || birdState == BIRD_DANCE) {
			bird.backgroundMusic.volume = 0.2f;
			bird.song.volume = 0;
		}

		yield return new WaitForSeconds(0.5f);
	}

	IEnumerator SampleSongCutsceneRoutine() {
		yield return StartCoroutine(InteractionRoutine(hotBird, BIRD_SING, THOUGHT_MUSIC));	
	}

    IEnumerator OpeningCutsceneRoutine() {
		yield return StartCoroutine(InteractionRoutine(bird, BIRD_TALK_HAPPY, THOUGHT_LOVE));
		yield return StartCoroutine(InteractionRoutine(hotBird, BIRD_SING, THOUGHT_MUSIC));
		yield return StartCoroutine(InteractionRoutine(bird, BIRD_SING, THOUGHT_SCRIBBLE));
		yield return StartCoroutine(InteractionRoutine(hotBird, BIRD_TALK_ANGRY, THOUGHT_SCRIBBLE));
		fallingLeaf.Fall();
		yield return new WaitForSeconds(5f);
		yield return StartCoroutine(InteractionRoutine(hotBird, BIRD_TALK_HAPPY, THOUGHT_LOVE));
		yield return StartCoroutine(InteractionRoutine(hotBird, BIRD_SING, THOUGHT_MUSIC));
    }

	IEnumerator GoodItemCutsceneRoutine() {
		yield return StartCoroutine(InteractionRoutine(hotBird, BIRD_TALK_HAPPY, THOUGHT_LOVE));
	}

    IEnumerator BadItemCutsceneRoutine() {
		yield return StartCoroutine(InteractionRoutine(hotBird, BIRD_TALK_ANGRY, THOUGHT_SCRIBBLE));
		// item.Fall();
    }

	IEnumerator EndingCutsceneRoutine() {
		yield return StartCoroutine(InteractionRoutine(bird, BIRD_TALK_HAPPY, THOUGHT_LOVE));
		bird.song = hotBird.song;
		yield return StartCoroutine(InteractionRoutine(bird, BIRD_SING, THOUGHT_MUSIC));
		yield return StartCoroutine(InteractionRoutine(hotBird, BIRD_TALK_HAPPY, THOUGHT_LOVE));
		StartCoroutine(InteractionRoutine(bird, BIRD_DANCE, THOUGHT_MUSIC));
		yield return StartCoroutine(InteractionRoutine(hotBird, BIRD_DANCE, THOUGHT_MUSIC));

		SceneManager.LoadScene("Credits", LoadSceneMode.Single);
	}

    IEnumerator FlyToBranch() {
		player.animating = true;
		Animator playerAnimator = player.GetComponent<Animator>();
		playerAnimator.SetBool("Stopped", false);

        Vector3 startingPosition = player.transform.position;
        float distance = (branchSitPosition.position - startingPosition).magnitude;
        float travelTime = 0.2f * distance;
        float timer = 0;
        while (timer < travelTime) {
            timer += Time.deltaTime;
            Vector3 newPosition = Vector3.Lerp(startingPosition, branchSitPosition.position, timer / travelTime);
            player.transform.position = new Vector3(newPosition.x, newPosition.y, player.transform.position.z);
            yield return null;
        }
        player.gameObject.SetActive(false);
        bird.gameObject.SetActive(true);
    }

    void ReleasePlayer() {
		Animator playerAnimator = player.GetComponent<Animator>();
		playerAnimator.SetBool("Stopped", true);		
		player.animating = false;
        bird.gameObject.SetActive(false);
		player.gameObject.SetActive(true);
    }

	public IEnumerator ScreenWipe(int origin) {
        var progress = 0f;
		var speed = 0.1f;
		transitionImage.fillOrigin = origin;
		var loops = 0;
		while (progress < 1 ) {
			progress += Time.deltaTime * (1f /speed);
			transitionImage.fillAmount = progress;
			loops++;
			yield return null;
		}
	}

	public IEnumerator ScreenUnwipe(int origin) {
		var progress = 1f;
		var speed = 0.1f;
		transitionImage.fillOrigin = origin;
		while (progress > 0) {
			progress -= Time.deltaTime * (1f /speed);
			transitionImage.fillAmount = progress;
			yield return null;
		}
	}
}
