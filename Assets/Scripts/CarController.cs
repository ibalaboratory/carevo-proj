// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {

    public Rigidbody CarModel;
    
	public void Stop() {
        CarModel.velocity = Vector3.zero;
        CarModel.angularVelocity = Vector3.zero;
        CarModel.ResetInertiaTensor();
        CarModel.isKinematic = true;
    }
}
