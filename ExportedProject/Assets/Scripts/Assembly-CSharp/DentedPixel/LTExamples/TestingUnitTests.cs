using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DentedPixel.LTExamples
{
	public class TestingUnitTests : MonoBehaviour
	{
		public GameObject cube1;

		public GameObject cube2;

		public GameObject cube3;

		public GameObject cube4;

		public GameObject cubeAlpha1;

		public GameObject cubeAlpha2;

		private bool eventGameObjectWasCalled;

		private bool eventGeneralWasCalled;

		private int lt1Id;

		private LTDescr lt2;

		private LTDescr lt3;

		private LTDescr lt4;

		private LTDescr[] groupTweens;

		private GameObject[] groupGOs;

		private int groupTweensCnt;

		private int rotateRepeat;

		private int rotateRepeatAngle;

		private GameObject boxNoCollider;

		private float timeElapsedNormalTimeScale;

		private float timeElapsedIgnoreTimeScale;

		private bool pauseTweenDidFinish;

		private void Awake()
		{
			boxNoCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
			UnityEngine.Object.Destroy(boxNoCollider.GetComponent(typeof(BoxCollider)));
		}

		private void Start()
		{
			LeanTest.timeout = 46f;
			LeanTest.expected = 62;
			LeanTween.init(1300);
			LeanTween.addListener(cube1, 0, eventGameObjectCalled);
			LeanTest.expect(!LeanTween.isTweening(), "NOTHING TWEEENING AT BEGINNING");
			LeanTest.expect(!LeanTween.isTweening(cube1), "OBJECT NOT TWEEENING AT BEGINNING");
			LeanTween.scaleX(cube4, 2f, 0f).setOnComplete((Action)delegate
			{
				LeanTest.expect(cube4.transform.localScale.x == 2f, "TWEENED WITH ZERO TIME");
			});
			LeanTween.dispatchEvent(0);
			LeanTest.expect(eventGameObjectWasCalled, "EVENT GAMEOBJECT RECEIVED");
			LeanTest.expect(!LeanTween.removeListener(cube2, 0, eventGameObjectCalled), "EVENT GAMEOBJECT NOT REMOVED");
			LeanTest.expect(LeanTween.removeListener(cube1, 0, eventGameObjectCalled), "EVENT GAMEOBJECT REMOVED");
			LeanTween.addListener(1, eventGeneralCalled);
			LeanTween.dispatchEvent(1);
			LeanTest.expect(eventGeneralWasCalled, "EVENT ALL RECEIVED");
			LeanTest.expect(LeanTween.removeListener(1, eventGeneralCalled), "EVENT ALL REMOVED");
			lt1Id = LeanTween.move(cube1, new Vector3(3f, 2f, 0.5f), 1.1f).id;
			LeanTween.move(cube2, new Vector3(-3f, -2f, -0.5f), 1.1f);
			LeanTween.reset();
			GameObject[] cubes = new GameObject[99];
			int[] tweenIds = new int[cubes.Length];
			for (int num = 0; num < cubes.Length; num++)
			{
				GameObject gameObject = cubeNamed("cancel" + num);
				tweenIds[num] = LeanTween.moveX(gameObject, 100f, 1f).id;
				cubes[num] = gameObject;
			}
			int onCompleteCount = 0;
			LeanTween.delayedCall(cubes[0], 0.2f, (Action)delegate
			{
				for (int i = 0; i < cubes.Length; i++)
				{
					if (i % 3 == 0)
					{
						LeanTween.cancel(cubes[i]);
					}
					else if (i % 3 == 1)
					{
						LeanTween.cancel(tweenIds[i]);
					}
					else if (i % 3 == 2)
					{
						LeanTween.descr(tweenIds[i]).setOnComplete((Action)delegate
						{
							onCompleteCount++;
							if (onCompleteCount >= 33)
							{
								LeanTest.expect(didPass: true, "CANCELS DO NOT EFFECT FINISHING");
							}
						});
					}
				}
			});
			new LTSpline(new Vector3[5]
			{
				new Vector3(-1f, 0f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(4f, 0f, 0f),
				new Vector3(20f, 0f, 0f),
				new Vector3(30f, 0f, 0f)
			}).place(cube4.transform, 0.5f);
			LeanTest.expect(Vector3.Distance(cube4.transform.position, new Vector3(10f, 0f, 0f)) <= 0.7f, "SPLINE POSITIONING AT HALFWAY", string.Concat("position is:", cube4.transform.position, " but should be:(10f,0f,0f)"));
			LeanTween.color(cube4, Color.green, 0.01f);
			GameObject gameObject2 = cubeNamed("cubeDest");
			Vector3 cubeDestEnd = new Vector3(100f, 20f, 0f);
			LeanTween.move(gameObject2, cubeDestEnd, 0.7f);
			GameObject cubeToTrans = cubeNamed("cubeToTrans");
			LeanTween.move(cubeToTrans, gameObject2.transform, 1.2f).setEase(LeanTweenType.easeOutQuad).setOnComplete((Action)delegate
			{
				LeanTest.expect(cubeToTrans.transform.position == cubeDestEnd, "MOVE TO TRANSFORM WORKS");
			});
			GameObject obj = cubeNamed("cubeDestroy");
			LeanTween.moveX(obj, 200f, 0.05f).setDelay(0.02f).setDestroyOnComplete(doesDestroy: true);
			LeanTween.moveX(obj, 200f, 0.1f).setDestroyOnComplete(doesDestroy: true).setOnComplete((Action)delegate
			{
				LeanTest.expect(didPass: true, "TWO DESTROY ON COMPLETE'S SUCCEED");
			});
			GameObject cubeSpline = cubeNamed("cubeSpline");
			LeanTween.moveSpline(cubeSpline, new Vector3[4]
			{
				new Vector3(0.5f, 0f, 0.5f),
				new Vector3(0.75f, 0f, 0.75f),
				new Vector3(1f, 0f, 1f),
				new Vector3(1f, 0f, 1f)
			}, 0.1f).setOnComplete((Action)delegate
			{
				LeanTest.expect(Vector3.Distance(new Vector3(1f, 0f, 1f), cubeSpline.transform.position) < 0.01f, "SPLINE WITH TWO POINTS SUCCEEDS");
			});
			GameObject jumpCube = cubeNamed("jumpTime");
			jumpCube.transform.position = new Vector3(100f, 0f, 0f);
			jumpCube.transform.localScale *= 100f;
			int jumpTimeId = LeanTween.moveX(jumpCube, 200f, 1f).id;
			LeanTween.delayedCall(base.gameObject, 0.2f, (Action)delegate
			{
				LTDescr lTDescr2 = LeanTween.descr(jumpTimeId);
				float beforeX = jumpCube.transform.position.x;
				lTDescr2.setTime(0.5f);
				LeanTween.delayedCall(0f, (Action)delegate
				{
				}).setOnStart(delegate
				{
					float num4 = 1f;
					beforeX += Time.deltaTime * 100f * 2f;
					LeanTest.expect(Mathf.Abs(jumpCube.transform.position.x - beforeX) < num4, "CHANGING TIME DOESN'T JUMP AHEAD", "Difference:" + Mathf.Abs(jumpCube.transform.position.x - beforeX) + " beforeX:" + beforeX + " now:" + jumpCube.transform.position.x + " dt:" + Time.deltaTime);
				});
			});
			GameObject zeroCube = cubeNamed("zeroCube");
			LeanTween.moveX(zeroCube, 10f, 0f).setOnComplete((Action)delegate
			{
				LeanTest.expect(zeroCube.transform.position.x == 10f, "ZERO TIME FINSHES CORRECTLY", "final x:" + zeroCube.transform.position.x);
			});
			GameObject cubeScale = cubeNamed("cubeScale");
			LeanTween.scale(cubeScale, new Vector3(5f, 5f, 5f), 0.01f).setOnStart(delegate
			{
				LeanTest.expect(didPass: true, "ON START WAS CALLED");
			}).setOnComplete((Action)delegate
			{
				LeanTest.expect(cubeScale.transform.localScale.z == 5f, "SCALE", "expected scale z:" + 5f + " returned:" + cubeScale.transform.localScale.z);
			});
			GameObject cubeRotate = cubeNamed("cubeRotate");
			LeanTween.rotate(cubeRotate, new Vector3(0f, 180f, 0f), 0.02f).setOnComplete((Action)delegate
			{
				LeanTest.expect(cubeRotate.transform.eulerAngles.y == 180f, "ROTATE", "expected rotate y:" + 180f + " returned:" + cubeRotate.transform.eulerAngles.y);
			});
			GameObject cubeRotateA = cubeNamed("cubeRotateA");
			LeanTween.rotateAround(cubeRotateA, Vector3.forward, 90f, 0.3f).setOnComplete((Action)delegate
			{
				LeanTest.expect(cubeRotateA.transform.eulerAngles.z == 90f, "ROTATE AROUND", "expected rotate z:" + 90f + " returned:" + cubeRotateA.transform.eulerAngles.z);
			});
			GameObject cubeRotateB = cubeNamed("cubeRotateB");
			cubeRotateB.transform.position = new Vector3(200f, 10f, 8f);
			LeanTween.rotateAround(cubeRotateB, Vector3.forward, 360f, 0.3f).setPoint(new Vector3(5f, 3f, 2f)).setOnComplete((Action)delegate
			{
				LeanTest.expect(cubeRotateB.transform.position.ToString() == new Vector3(200f, 10f, 8f).ToString(), "ROTATE AROUND 360", string.Concat("expected rotate pos:", new Vector3(200f, 10f, 8f), " returned:", cubeRotateB.transform.position));
			});
			LeanTween.alpha(cubeAlpha1, 0.5f, 0.1f).setOnUpdate(delegate(float val)
			{
				LeanTest.expect(val != 0f, "ON UPDATE VAL");
			}).setOnCompleteParam("Hi!")
				.setOnComplete(delegate(object completeObj)
				{
					LeanTest.expect((string)completeObj == "Hi!", "ONCOMPLETE OBJECT");
					LeanTest.expect(cubeAlpha1.GetComponent<Renderer>().material.color.a == 0.5f, "ALPHA");
				});
			float onStartTime = -1f;
			LeanTween.color(cubeAlpha2, Color.cyan, 0.3f).setOnComplete((Action)delegate
			{
				LeanTest.expect(cubeAlpha2.GetComponent<Renderer>().material.color == Color.cyan, "COLOR");
				LeanTest.expect(onStartTime >= 0f && onStartTime < Time.time, "ON START", "onStartTime:" + onStartTime + " time:" + Time.time);
			}).setOnStart(delegate
			{
				onStartTime = Time.time;
			});
			Vector3 beforePos = cubeAlpha1.transform.position;
			LeanTween.moveY(cubeAlpha1, 3f, 0.2f).setOnComplete((Action)delegate
			{
				LeanTest.expect(cubeAlpha1.transform.position.x == beforePos.x && cubeAlpha1.transform.position.z == beforePos.z, "MOVE Y");
			});
			Vector3 beforePos2 = cubeAlpha2.transform.localPosition;
			LeanTween.moveLocalZ(cubeAlpha2, 12f, 0.2f).setOnComplete((Action)delegate
			{
				LeanTest.expect(cubeAlpha2.transform.localPosition.x == beforePos2.x && cubeAlpha2.transform.localPosition.y == beforePos2.y, "MOVE LOCAL Z", "ax:" + cubeAlpha2.transform.localPosition.x + " bx:" + beforePos.x + " ay:" + cubeAlpha2.transform.localPosition.y + " by:" + beforePos2.y);
			});
			AudioClip audio = LeanAudio.createAudio(new AnimationCurve(new Keyframe(0f, 1f, 0f, -1f), new Keyframe(1f, 0f, -1f, 0f)), new AnimationCurve(new Keyframe(0f, 0.001f, 0f, 0f), new Keyframe(1f, 0.001f, 0f, 0f)), LeanAudio.options());
			LeanTween.delayedSound(base.gameObject, audio, new Vector3(0f, 0f, 0f), 0.1f).setDelay(0.2f).setOnComplete((Action)delegate
			{
				LeanTest.expect(Time.time > 0f, "DELAYED SOUND");
			});
			int totalEasingCheck = 0;
			int totalEasingCheckSuccess = 0;
			for (int num2 = 0; num2 < 2; num2++)
			{
				bool flag = num2 == 1;
				int totalTweenTypeLength = 33;
				for (int num3 = 0; num3 < totalTweenTypeLength; num3++)
				{
					LeanTweenType leanTweenType = (LeanTweenType)num3;
					GameObject onCompleteParam = cubeNamed("cube" + leanTweenType);
					LTDescr lTDescr = LeanTween.moveLocalX(onCompleteParam, 5f, 0.1f).setOnComplete(delegate(object obj3)
					{
						GameObject obj2 = obj3 as GameObject;
						int num4 = totalEasingCheck;
						totalEasingCheck = num4 + 1;
						if (obj2.transform.position.x == 5f)
						{
							num4 = totalEasingCheckSuccess;
							totalEasingCheckSuccess = num4 + 1;
						}
						if (totalEasingCheck == 2 * totalTweenTypeLength)
						{
							LeanTest.expect(totalEasingCheck == totalEasingCheckSuccess, "EASING TYPES");
						}
					}).setOnCompleteParam(onCompleteParam);
					if (flag)
					{
						lTDescr.setFrom(-5f);
					}
				}
			}
			bool value2UpdateCalled = false;
			LeanTween.value(base.gameObject, new Vector2(0f, 0f), new Vector2(256f, 96f), 0.1f).setOnUpdate((Action<Vector2>)delegate
			{
				value2UpdateCalled = true;
			}, (object)null);
			LeanTween.delayedCall(0.2f, (Action)delegate
			{
				LeanTest.expect(value2UpdateCalled, "VALUE2 UPDATE");
			});
			StartCoroutine(timeBasedTesting());
		}

		private GameObject cubeNamed(string name)
		{
			GameObject obj = UnityEngine.Object.Instantiate(boxNoCollider);
			obj.name = name;
			return obj;
		}

		private IEnumerator timeBasedTesting()
		{
			yield return new WaitForEndOfFrame();
			GameObject obj = cubeNamed("normalTimeScale");
			LeanTween.moveX(obj, 12f, 1.5f).setIgnoreTimeScale(useUnScaledTime: false).setOnComplete((Action)delegate
			{
				timeElapsedNormalTimeScale = Time.time;
			});
			LTDescr[] array = LeanTween.descriptions(obj);
			LeanTest.expect(array.Length >= 0 && array[0].to.x == 12f, "WE CAN RETRIEVE A DESCRIPTION");
			LeanTween.moveX(cubeNamed("ignoreTimeScale"), 5f, 1.5f).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete((Action)delegate
			{
				timeElapsedIgnoreTimeScale = Time.time;
			});
			yield return new WaitForSeconds(1.5f);
			LeanTest.expect(Mathf.Abs(timeElapsedNormalTimeScale - timeElapsedIgnoreTimeScale) < 0.7f, "START IGNORE TIMING", "timeElapsedIgnoreTimeScale:" + timeElapsedIgnoreTimeScale + " timeElapsedNormalTimeScale:" + timeElapsedNormalTimeScale);
			Time.timeScale = 4f;
			int pauseCount = 0;
			LeanTween.value(base.gameObject, 0f, 1f, 1f).setOnUpdate((Action<float>)delegate
			{
				pauseCount++;
			}).pause();
			Vector3[] array2 = new Vector3[16]
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(-9.1f, 25.1f, 0f),
				new Vector3(-1.2f, 15.9f, 0f),
				new Vector3(-25f, 25f, 0f),
				new Vector3(-25f, 25f, 0f),
				new Vector3(-50.1f, 15.9f, 0f),
				new Vector3(-40.9f, 25.1f, 0f),
				new Vector3(-50f, 0f, 0f),
				new Vector3(-50f, 0f, 0f),
				new Vector3(-40.9f, -25.1f, 0f),
				new Vector3(-50.1f, -15.9f, 0f),
				new Vector3(-25f, -25f, 0f),
				new Vector3(-25f, -25f, 0f),
				new Vector3(0f, -15.9f, 0f),
				new Vector3(-9.1f, -25.1f, 0f),
				new Vector3(0f, 0f, 0f)
			};
			GameObject cubeRound = cubeNamed("bRound");
			Vector3 onStartPos = cubeRound.transform.position;
			LeanTween.moveLocal(cubeRound, array2, 0.5f).setOnComplete((Action)delegate
			{
				LeanTest.expect(cubeRound.transform.position == onStartPos, "BEZIER CLOSED LOOP SHOULD END AT START", string.Concat("onStartPos:", onStartPos, " onEnd:", cubeRound.transform.position));
			});
			LeanTest.expect(object.Equals(new LTBezierPath(array2).ratioAtPoint(new Vector3(-25f, 25f, 0f)), 0.25f), "BEZIER RATIO POINT");
			Vector3[] to = new Vector3[6]
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(2f, 0f, 0f),
				new Vector3(0.9f, 2f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 0f, 0f)
			};
			GameObject cubeSpline = cubeNamed("bSpline");
			Vector3 onStartPosSpline = cubeSpline.transform.position;
			LeanTween.moveSplineLocal(cubeSpline, to, 0.5f).setOnComplete((Action)delegate
			{
				LeanTest.expect(Vector3.Distance(onStartPosSpline, cubeSpline.transform.position) <= 0.01f, "SPLINE CLOSED LOOP SHOULD END AT START", string.Concat("onStartPos:", onStartPosSpline, " onEnd:", cubeSpline.transform.position, " dist:", Vector3.Distance(onStartPosSpline, cubeSpline.transform.position)));
			});
			GameObject cubeSeq = cubeNamed("cSeq");
			LTSeq lTSeq = LeanTween.sequence().append(LeanTween.moveX(cubeSeq, 100f, 0.2f));
			lTSeq.append(0.1f).append(LeanTween.scaleX(cubeSeq, 2f, 0.1f));
			lTSeq.append(delegate
			{
				LeanTest.expect(cubeSeq.transform.position.x == 100f, "SEQ MOVE X FINISHED", "move x:" + cubeSeq.transform.position.x);
				LeanTest.expect(cubeSeq.transform.localScale.x == 2f, "SEQ SCALE X FINISHED", "scale x:" + cubeSeq.transform.localScale.x);
			}).setScale(0.2f);
			GameObject cubeBounds = cubeNamed("cBounds");
			bool didPassBounds = true;
			Vector3 failPoint = Vector3.zero;
			LeanTween.move(cubeBounds, new Vector3(10f, 10f, 10f), 0.1f).setOnUpdate((Action<float>)delegate
			{
				if (cubeBounds.transform.position.x < 0f || cubeBounds.transform.position.x > 10f || cubeBounds.transform.position.y < 0f || cubeBounds.transform.position.y > 10f || cubeBounds.transform.position.z < 0f || cubeBounds.transform.position.z > 10f)
				{
					didPassBounds = false;
					failPoint = cubeBounds.transform.position;
				}
			}).setLoopPingPong()
				.setRepeat(8)
				.setOnComplete((Action)delegate
				{
					LeanTest.expect(didPassBounds, "OUT OF BOUNDS", "pos x:" + failPoint.x + " y:" + failPoint.y + " z:" + failPoint.z);
				});
			groupTweens = new LTDescr[1200];
			groupGOs = new GameObject[groupTweens.Length];
			groupTweensCnt = 0;
			int descriptionMatchCount = 0;
			for (int num = 0; num < groupTweens.Length; num++)
			{
				GameObject gameObject = cubeNamed("c" + num);
				gameObject.transform.position = new Vector3(0f, 0f, num * 3);
				groupGOs[num] = gameObject;
			}
			yield return new WaitForEndOfFrame();
			bool hasGroupTweensCheckStarted = false;
			int setOnStartNum = 0;
			int setPosNum = 0;
			bool setPosOnUpdate = true;
			for (int num2 = 0; num2 < groupTweens.Length; num2++)
			{
				Vector3 vector = base.transform.position + Vector3.one * 3f;
				Dictionary<string, object> onCompleteParam = new Dictionary<string, object>
				{
					{ "final", vector },
					{
						"go",
						groupGOs[num2]
					}
				};
				groupTweens[num2] = LeanTween.move(groupGOs[num2], vector, 3f).setOnStart(delegate
				{
					setOnStartNum++;
				}).setOnUpdate(delegate(Vector3 newPosition)
				{
					if (base.transform.position.z > newPosition.z)
					{
						setPosOnUpdate = false;
					}
				})
					.setOnCompleteParam(onCompleteParam)
					.setOnComplete(delegate(object param)
					{
						Dictionary<string, object> obj2 = param as Dictionary<string, object>;
						Vector3 vector2 = (Vector3)obj2["final"];
						GameObject gameObject2 = obj2["go"] as GameObject;
						if (vector2.ToString() == gameObject2.transform.position.ToString())
						{
							setPosNum++;
						}
						if (!hasGroupTweensCheckStarted)
						{
							hasGroupTweensCheckStarted = true;
							LeanTween.delayedCall(base.gameObject, 0.1f, (Action)delegate
							{
								LeanTest.expect(setOnStartNum == groupTweens.Length, "SETONSTART CALLS", "expected:" + groupTweens.Length + " was:" + setOnStartNum);
								LeanTest.expect(groupTweensCnt == groupTweens.Length, "GROUP FINISH", "expected " + groupTweens.Length + " tweens but got " + groupTweensCnt);
								LeanTest.expect(setPosNum == groupTweens.Length, "GROUP POSITION FINISH", "expected " + groupTweens.Length + " tweens but got " + setPosNum);
								LeanTest.expect(setPosOnUpdate, "GROUP POSITION ON UPDATE");
							});
						}
						groupTweensCnt++;
					});
				if (LeanTween.description(groupTweens[num2].id).trans == groupTweens[num2].trans)
				{
					descriptionMatchCount++;
				}
			}
			while (LeanTween.tweensRunning < groupTweens.Length)
			{
				yield return null;
			}
			LeanTest.expect(descriptionMatchCount == groupTweens.Length, "GROUP IDS MATCH");
			int num3 = groupTweens.Length + 7;
			LeanTest.expect(LeanTween.maxSearch <= num3, "MAX SEARCH OPTIMIZED", "maxSearch:" + LeanTween.maxSearch + " should be:" + num3);
			LeanTest.expect(LeanTween.isTweening(), "SOMETHING IS TWEENING");
			float previousXlt4 = cube4.transform.position.x;
			lt4 = LeanTween.moveX(cube4, 5f, 1.1f).setOnComplete((Action)delegate
			{
				LeanTest.expect(cube4 != null && previousXlt4 != cube4.transform.position.x, "RESUME OUT OF ORDER", string.Concat("cube4:", cube4, " previousXlt4:", previousXlt4, " cube4.transform.position.x:", (cube4 != null) ? cube4.transform.position.x : 0f));
			}).setDestroyOnComplete(doesDestroy: true);
			lt4.resume();
			TestingUnitTests testingUnitTests = this;
			TestingUnitTests testingUnitTests2 = this;
			int num4 = 0;
			testingUnitTests2.rotateRepeatAngle = 0;
			testingUnitTests.rotateRepeat = num4;
			LeanTween.rotateAround(cube3, Vector3.forward, 360f, 0.1f).setRepeat(3).setOnComplete(rotateRepeatFinished)
				.setOnCompleteOnRepeat(isOn: true)
				.setDestroyOnComplete(doesDestroy: true);
			yield return new WaitForEndOfFrame();
			LeanTween.delayedCall(1.8f, rotateRepeatAllFinished);
			int tweensRunning = LeanTween.tweensRunning;
			LeanTween.cancel(lt1Id);
			LeanTest.expect(tweensRunning == LeanTween.tweensRunning, "CANCEL AFTER RESET SHOULD FAIL", "expected " + tweensRunning + " but got " + LeanTween.tweensRunning);
			LeanTween.cancel(cube2);
			int num5 = 0;
			for (int num6 = 0; num6 < groupTweens.Length; num6++)
			{
				if (LeanTween.isTweening(groupGOs[num6]))
				{
					num5++;
				}
				if (num6 % 3 == 0)
				{
					LeanTween.pause(groupGOs[num6]);
				}
				else if (num6 % 3 == 1)
				{
					groupTweens[num6].pause();
				}
				else
				{
					LeanTween.pause(groupTweens[num6].id);
				}
			}
			LeanTest.expect(num5 == groupTweens.Length, "GROUP ISTWEENING", "expected " + groupTweens.Length + " tweens but got " + num5);
			yield return new WaitForEndOfFrame();
			num5 = 0;
			for (int num7 = 0; num7 < groupTweens.Length; num7++)
			{
				if (num7 % 3 == 0)
				{
					LeanTween.resume(groupGOs[num7]);
				}
				else if (num7 % 3 == 1)
				{
					groupTweens[num7].resume();
				}
				else
				{
					LeanTween.resume(groupTweens[num7].id);
				}
				if ((num7 % 2 == 0) ? LeanTween.isTweening(groupTweens[num7].id) : LeanTween.isTweening(groupGOs[num7]))
				{
					num5++;
				}
			}
			LeanTest.expect(num5 == groupTweens.Length, "GROUP RESUME");
			LeanTest.expect(!LeanTween.isTweening(cube1), "CANCEL TWEEN LTDESCR");
			LeanTest.expect(!LeanTween.isTweening(cube2), "CANCEL TWEEN LEANTWEEN");
			LeanTest.expect(pauseCount == 0, "ON UPDATE NOT CALLED DURING PAUSE", "expect pause count of 0, but got " + pauseCount);
			yield return new WaitForEndOfFrame();
			Time.timeScale = 0.25f;
			float num8 = 0.2f;
			float expectedTime = num8 * (1f / Time.timeScale);
			float start = Time.realtimeSinceStartup;
			bool onUpdateWasCalled = false;
			LeanTween.moveX(cube1, -5f, num8).setOnUpdate((Action<float>)delegate
			{
				onUpdateWasCalled = true;
			}).setOnComplete((Action)delegate
			{
				float num10 = Time.realtimeSinceStartup - start;
				LeanTest.expect(Mathf.Abs(expectedTime - num10) < 0.06f, "SCALED TIMING DIFFERENCE", "expected to complete in roughly " + expectedTime + " but completed in " + num10);
				LeanTest.expect(Mathf.Approximately(cube1.transform.position.x, -5f), "SCALED ENDING POSITION", "expected to end at -5f, but it ended at " + cube1.transform.position.x);
				LeanTest.expect(onUpdateWasCalled, "ON UPDATE FIRED");
			});
			bool didGetCorrectOnUpdate = false;
			LeanTween.value(base.gameObject, new Vector3(1f, 1f, 1f), new Vector3(10f, 10f, 10f), 1f).setOnUpdate(delegate(Vector3 val)
			{
				didGetCorrectOnUpdate = val.x >= 1f && val.y >= 1f && val.z >= 1f;
			}).setOnComplete((Action)delegate
			{
				LeanTest.expect(didGetCorrectOnUpdate, "VECTOR3 CALLBACK CALLED");
			});
			yield return new WaitForSeconds(expectedTime);
			Time.timeScale = 1f;
			int num9 = 0;
			GameObject[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			for (num4 = 0; num4 < array3.Length; num4++)
			{
				if (array3[num4].name == "~LeanTween")
				{
					num9++;
				}
			}
			LeanTest.expect(num9 == 1, "RESET CORRECTLY CLEANS UP");
			StartCoroutine(lotsOfCancels());
		}

		private IEnumerator lotsOfCancels()
		{
			yield return new WaitForEndOfFrame();
			Time.timeScale = 4f;
			int cubeCount = 10;
			int[] tweensA = new int[cubeCount];
			GameObject[] aGOs = new GameObject[cubeCount];
			for (int i = 0; i < aGOs.Length; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(boxNoCollider);
				gameObject.transform.position = new Vector3(0f, 0f, (float)i * 2f);
				gameObject.name = "a" + i;
				aGOs[i] = gameObject;
				tweensA[i] = LeanTween.move(gameObject, gameObject.transform.position + new Vector3(10f, 0f, 0f), 0.5f + 1f * (1f / (float)aGOs.Length)).id;
				LeanTween.color(gameObject, Color.red, 0.01f);
			}
			yield return new WaitForSeconds(1f);
			int[] tweensB = new int[cubeCount];
			GameObject[] bGOs = new GameObject[cubeCount];
			for (int j = 0; j < bGOs.Length; j++)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(boxNoCollider);
				gameObject2.transform.position = new Vector3(0f, 0f, (float)j * 2f);
				gameObject2.name = "b" + j;
				bGOs[j] = gameObject2;
				tweensB[j] = LeanTween.move(gameObject2, gameObject2.transform.position + new Vector3(10f, 0f, 0f), 2f).id;
			}
			for (int k = 0; k < aGOs.Length; k++)
			{
				LeanTween.cancel(aGOs[k]);
				GameObject gameObject3 = aGOs[k];
				tweensA[k] = LeanTween.move(gameObject3, new Vector3(0f, 0f, (float)k * 2f), 2f).id;
			}
			yield return new WaitForSeconds(0.5f);
			for (int l = 0; l < aGOs.Length; l++)
			{
				LeanTween.cancel(aGOs[l]);
				GameObject gameObject4 = aGOs[l];
				tweensA[l] = LeanTween.move(gameObject4, new Vector3(0f, 0f, (float)l * 2f) + new Vector3(10f, 0f, 0f), 2f).id;
			}
			for (int m = 0; m < bGOs.Length; m++)
			{
				LeanTween.cancel(bGOs[m]);
				GameObject gameObject5 = bGOs[m];
				tweensB[m] = LeanTween.move(gameObject5, new Vector3(0f, 0f, (float)m * 2f), 2f).id;
			}
			yield return new WaitForSeconds(2.1f);
			bool didPass = true;
			for (int n = 0; n < aGOs.Length; n++)
			{
				if (Vector3.Distance(aGOs[n].transform.position, new Vector3(0f, 0f, (float)n * 2f) + new Vector3(10f, 0f, 0f)) > 0.1f)
				{
					didPass = false;
				}
			}
			for (int num = 0; num < bGOs.Length; num++)
			{
				if (Vector3.Distance(bGOs[num].transform.position, new Vector3(0f, 0f, (float)num * 2f)) > 0.1f)
				{
					didPass = false;
				}
			}
			LeanTest.expect(didPass, "AFTER LOTS OF CANCELS");
			cubeNamed("cPaused").LeanMoveX(10f, 1f).setOnComplete((Action)delegate
			{
				pauseTweenDidFinish = true;
			});
			StartCoroutine(pauseTimeNow());
		}

		private IEnumerator pauseTimeNow()
		{
			yield return new WaitForSeconds(0.5f);
			Time.timeScale = 0f;
			LeanTween.delayedCall(0.5f, (Action)delegate
			{
				Time.timeScale = 1f;
			}).setUseEstimatedTime(useEstimatedTime: true);
			LeanTween.delayedCall(1.5f, (Action)delegate
			{
				LeanTest.expect(pauseTweenDidFinish, "PAUSE BY TIMESCALE FINISHES");
			}).setUseEstimatedTime(useEstimatedTime: true);
		}

		private void rotateRepeatFinished()
		{
			if (Mathf.Abs(cube3.transform.eulerAngles.z) < 0.0001f)
			{
				rotateRepeatAngle++;
			}
			rotateRepeat++;
		}

		private void rotateRepeatAllFinished()
		{
			LeanTest.expect(rotateRepeatAngle == 3, "ROTATE AROUND MULTIPLE", "expected 3 times received " + rotateRepeatAngle + " times");
			LeanTest.expect(rotateRepeat == 3, "ROTATE REPEAT", "expected 3 times received " + rotateRepeat + " times");
			LeanTest.expect(cube3 == null, "DESTROY ON COMPLETE", "cube3:" + cube3);
		}

		private void eventGameObjectCalled(LTEvent e)
		{
			eventGameObjectWasCalled = true;
		}

		private void eventGeneralCalled(LTEvent e)
		{
			eventGeneralWasCalled = true;
		}
	}
}
