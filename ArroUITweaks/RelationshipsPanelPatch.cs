using MonoPatcherLib;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.CAS;
using Sims3.UI.Hud;

namespace Arro.UITweaks
{
    public class RelationshipsPanelPatch
    {
        [ReplaceMethod(typeof(RelationshipsPanel), "OnSimNotOnLotMouseUp")]
        public void OnSimNotOnLotMouseUp(WindowBase sender, UIMouseEventArgs eventArgs)
        {
            var relationshipsPanel = (RelationshipsPanel)(this as object);
            Window window = sender as Window; 
            if (eventArgs.MouseKey == MouseKeys.kMouseLeft)
            {
                bool flag2 = window != null && window.Parent != null;
                if (flag2)
                {
                    IMiniSimDescription miniSimDescription = window.Parent.Tag as IMiniSimDescription;
                    bool flag3 = miniSimDescription != null;
                    if (flag3)
                    {
                        miniSimDescription.OnPickFromPanel(eventArgs, new GameObjectHit(GameObjectHitType.Object));
                    }
                }
            }
            else
            { 
                if (eventArgs.MouseKey == MouseKeys.kMouseRight)
                {
                    ObjectGuid objectGuid = (window.Tag == null) ? ObjectGuid.InvalidObjectGuid : ((ObjectGuid)window.Tag);
                    bool flag5 = objectGuid != ObjectGuid.InvalidObjectGuid;
                    if (flag5)
                    {
                        ICameraModel cameraModel = Responder.Instance.CameraModel;
                        Vector3 objectWorldPosition = cameraModel.GetObjectWorldPosition(objectGuid);
                        if (objectWorldPosition != Vector3.OutOfWorld)
                        {
                            cameraModel.FocusOnGivenPosition(objectWorldPosition, 1f);
                        }
                    }
                }
            }
        }
    }
}