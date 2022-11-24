using DCL;
using Decentraland.Bff;

namespace DCLPlugins.RealmsPlugin
{
    public class RealmsMinimapModifier : IRealmsModifier
    {
        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        {
            DataStore.i.HUDs.minimapVisible.Set(realmConfiguration.Configurations.Minimap.Enabled);
            DataStore.i.HUDs.jumpHomeButtonVisible.Set(!isCatalyst);
        }

        public void Dispose() { }

    }
}