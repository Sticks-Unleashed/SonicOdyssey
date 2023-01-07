using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sky : MonoBehaviour
{
[SerializeField] Transform mainCam;
void LateUpdate()
{
  transform.rotation = mainCam.rotation;

}
}
