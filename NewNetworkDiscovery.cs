using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewNetworkDiscovery : NetworkDiscoveryBase
{
    #region Server

    protected override void ProcessClientRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
        base.ProcessClientRequest(request, endpoint);
    }

    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
        // Create a response and return it   
        return new DiscoveryResponse();
    }

    #endregion

    #region Client

    protected override DiscoveryRequest GetRequest()
    {
        return new DiscoveryRequest();
    }

    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
    {
        // a server replied,  invoke a unity event (notification UI) with a response
    }

    #endregion
}
