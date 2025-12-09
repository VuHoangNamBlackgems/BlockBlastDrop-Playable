using System.Collections;
using System.Linq;
//using datmonolib;
//using PrimeTween;
using UnityEngine;

public class ColorVacoomBoosterController : AbBoosterController
{
//    [SerializeField] private AudioClipInfoSO _magnetClip;
  //  private IRequestObj _requestObj;
    [SerializeField] private Transform _magnetTf;
    [SerializeField] private Transform _blocksHolder;
    private bool _couldCancel = true;
    private void OnEnable()
    {
        /*
        transform.position = new Vector3(0,0, Camera.main.transform.position.z - 0.6f);
        */
        StartUseBooster();
    }
    private void OnDisable()
    {
      //  _requestObj.Finish();
    }

    public override void StartUseBooster()
    {
        base.StartUseBooster();
      //  StartCoroutine(IsUseBooster());
    }

    public override void CancelUsingBooster()
    {
        if(!CouldCancel()) return;
        base.CancelUsingBooster();
     /*   var boosterButtonControllers=Obj.Gets<UIButtonBoosterController>();
        var matchBoosterUIButton=boosterButtonControllers.First(v =>
        {
            return v.BoosterDataSo == _boosterData;
        });
        matchBoosterUIButton.ToDefaultState();*/
        StopAllCoroutines();
       /* var multiBlocks = Obj.Gets<MultiBlockController>();
        multiBlocks = multiBlocks.Where(v =>
        {
            return v.CouldDestroyByColorVacuum();
        }).ToArray();
        foreach (var multiBlock in multiBlocks)
        {
            multiBlock.MainBlock.DisableOutLine();
        }
        Obj.DestroySingle(gameObject);*/
    }

    public override bool CouldCancel()
    {
        return _couldCancel;
    }

    public override void StopUsingBooster()
    {
        base.StopUsingBooster();
       /* var boosterButtonController=Obj.Gets<UIButtonBoosterController>();
        var matchBoosterUIButton=boosterButtonController.First(v =>
        {
            return v.BoosterDataSo == _boosterData;
        });
        matchBoosterUIButton.ToDefaultState();
        StopAllCoroutines();
        var multiBlocks = Obj.Gets<MultiBlockController>();
        multiBlocks = multiBlocks.Where(v =>
        {
            return v.CouldDestroyByColorVacuum();
        }).ToArray();
        foreach (var multiBlock in multiBlocks)
        {
            multiBlock.MainBlock.DisableOutLine();
        }    
        Obj.DestroySingle(gameObject);*/
    }

   /* private IEnumerator IsUseBooster()
    {
        var startPosition=_magnetTf.position;
        *//*
        _magnetTf.position=_magnetTf.position+Vector3.left*20;
        */
        /*
        _magnetTf.DOMove(startPosition, 0.5f).SetEase(Ease.InOutQuad);
        */
        
        /*
        _magnetTf.position = new Vector3(startPosition.x,Camera.main.transform.position.y-10, Camera.main.transform.position.z+16);
        *//*
        _magnetTf.position += startPosition + Vector3.up * 7;
        var newPosition=_magnetTf.position;
        Tween.PositionAtSpeed(_magnetTf, startPosition, 35,ease:Ease.OutQuad);
        var boosterButtonController=Obj.Gets<UIButtonBoosterController>();
        var matchBoosterUIButton=boosterButtonController.First(v =>
        {
            return v.BoosterDataSo == _boosterData;
        });
        matchBoosterUIButton.ToUsingState();
        var multiBlocks = Obj.Gets<MultiBlockController>();
        multiBlocks = multiBlocks.Where(v =>
        {
            return v.CouldDestroyByColorVacuum();
        }).ToArray();
        foreach (var multiBlock in multiBlocks)
        {
            multiBlock.MainBlock.EnableVisibleOutLine();
        }
        _requestObj=Game.MainLogicRequestControl.CreateGoRequestCycle(gameObject);
        while (true)
        {
            if (TouchEvent.JustClicked&&!Game.SubLogicRequestControl.IsHasRequest)
            {
                TouchEvent.TryGetNewestTouch(out var touch);
                var ray = Camera.main.ScreenPointToRay(touch.ScreenPosition());
                var isHit=Physics.Raycast(ray, out var hitInfo,1000);
                if (isHit)
                {
                    if (hitInfo.rigidbody != null)
                    {
                        var isMultiBlockType = hitInfo.rigidbody.TryGetComponent<MultiBlockController>(out var multiBlockController);
                        if (isMultiBlockType)
                        {
                            var parentBlock = hitInfo.collider.GetComponentInParent<BlockController>();
                            if (!parentBlock.IsHideColor)
                            {
                                if (multiBlockController.CouldDestroyByColorVacuum())
                                {
                                    var color = parentBlock.ColorDataSo;
                                    var matchMultiBlocks = multiBlocks.Where(v =>
                                    {
                                        return v.CouldDestroyByColorVacuum(color);
                                    }).ToList();
                                    foreach (var block in multiBlocks)
                                    {
                                        if (!matchMultiBlocks.Contains(block))
                                        {
                                            block.MainBlock.DisableOutLine();
                                        }
                                    }
                                var targetOffset = 0f;
                                _couldCancel = false;
                                
                                matchMultiBlocks = matchMultiBlocks.OrderBy(v =>
                                {
                                    return Vector3.Distance(v.MainBlock.CenterTf.position, _blocksHolder.position);
                                }).ToList();
                                GamePlayManager.Ins.EffectRequestControl.AddRequest();
                                foreach (var multiBlock in matchMultiBlocks)
                                {
                                    multiBlock.DestroyByColorVacuum(color);
                                }

                                AudioManager.Ins.PlayAudio(_magnetClip).FadeOut(0.45f);
                                yield return new WaitForSeconds(0.25f);
                                var toTime = Time.time;
                                for (int i = 0; i < matchMultiBlocks.Count; i++)
                                {
                                    var multiBlock = matchMultiBlocks[i];
                                    var blockMatchs = multiBlock.GetVocuumBlocksByColor(color);
                                    *//*
                                    multiBlock.MoveTransform.SetParent(_blocksHolder);
                                    *//*
                                    if (i == 0)
                                    {
                                        targetOffset += blockMatchs.Count < 2 ? 1.443f : 2.45f;
                                    }
                                    else
                                    {
                                        targetOffset += blockMatchs.Count < 2 ? 1.284f : 1.88f;
                                    }
                                    if (blockMatchs.Count == multiBlock.BlockControllers.Count)
                                    {
                                        multiBlock.MoveTransform.SetParent(_blocksHolder);
                                        var tween = Tween.LocalPositionAtSpeed(multiBlock.MoveTransform, Vector3.zero +
                                            Vector3.down * targetOffset, 30,ease:Ease.InQuad);
                                        var duration = tween.duration*0.8f;
                                        Tween.LocalRotation(multiBlock.MoveTransform, new Vector3(0,UnityEngine.Random.Range(180,360),0), duration);
                                        toTime=Time.time+duration;
                                        GamePlayManager.Ins.InvokeBlockMoveOut();
                                    }
                                    else
                                    {
                                        blockMatchs[0].MoveTransform.SetParent(_blocksHolder);
                                        var tween = Tween.LocalPositionAtSpeed(blockMatchs[0].MoveTransform,
                                            Vector3.zero +
                                            Vector3.down * targetOffset, 30,ease:Ease.InQuad);
                                        var duration = tween.duration*0.8f;
                                        Tween.LocalRotation(blockMatchs[0].MoveTransform, new Vector3(0,UnityEngine.Random.Range(180,360),0), duration);
                                        if (blockMatchs[0] != multiBlock.MainBlock)
                                        {
                                            multiBlock.MainBlock.DisableOutLine();
                                            blockMatchs[0].EnableVisibleOutLine();
                                        }
                                        toTime=Time.time+duration;
                                        ScheduleManager.Ins.Schedule(this, 0.1f, () =>
                                        {
                                            multiBlock.RemoveBlock(blockMatchs[0]);
                                        });
                                        *//*
                                        yield return new WaitForSeconds(0.2f);
                                    *//*
                                    }
                                    yield return new WaitForSeconds(0.2f);
                                }
                                yield return new WaitForSeconds(0.2f);
                                Tween
                                    .PositionAtSpeed(_magnetTf, newPosition+Vector3.up*5+Vector3.forward*5,
                                        28, ease: Ease.InOutQuad);
                                var screenWph = ScreenExtensions.WidthHeightPercent;
                                yield return new WaitForSeconds(0.4f);
                                
                                if (screenWph > 0.6f)
                                {
                                    yield return new WaitForSeconds(0.45f);
                                }
                                else
                                {
                                    yield return new WaitForSeconds(0.35f);
                                }
                                GamePlayManager.Ins.EffectRequestControl.RemoveRequest();
                                *//*foreach (var multiBlock in matchMultiBlocks)
                                {
                                    multiBlock.DestroyByColorVacuum(color);
                                }*//*
                                // destroy
                                UseBoosterSuccess();
                                StopUsingBooster();
                                yield break;
                            }

                            }

                        }
                    }
                }
            }
            yield return null;
        }
    }*/
}
