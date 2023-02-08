using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Util
{
    public class TweenLibrary : MonoBehaviour
    {
        public LlistaAccio llistat;
        private Transform transformComponent;

        [System.Serializable]
        public struct LlistaAccio
        {
            public Accio[] llista;
        }

        [System.Serializable]
        public struct Accio
        {
            public UnityEvent function;
            public float timeToNext;
        }

        // Start is called before the first frame update
        void Start()
        {
            transformComponent = GetComponent<Transform>();
        }



        //Scale ------------------------------------------------------------------
        public void Scale(float x, float y, float z)
        {
            Scale(new Vector3(x,y,z));
        }

        public void Scale(Vector3 v3)
        {
            transformComponent.localScale = v3;
        }

        public void XScale(float n) => Scale(n, transformComponent.localScale.y, transformComponent.localScale.z);
        public void YScale(float n) => Scale(transformComponent.localScale.x, n, transformComponent.localScale.z);
        public void ZScale(float n) => Scale(transformComponent.localScale.x, transformComponent.localScale.y, n);

        //Move ------------------------------------------------------------------
        public void Move(float x, float y, float z)
        {
            Move(new Vector3(x, y, z));
        }

        public void Move(Vector3 v3)
        {
            transformComponent.position = v3;
        }

        public void XMove(float n) => Move(n, transformComponent.position.y, transformComponent.position.z);
        public void YMove(float n) => Move(transformComponent.position.x, n, transformComponent.position.z);
        public void ZMove(float n) => Move(transformComponent.position.x, transformComponent.position.y, n);

        public void LocalMove(float x, float y, float z)
        {
            LocalMove(new Vector3(x, y, z));
        }

        public void LocalMove(Vector3 v3)
        {
            transformComponent.position = v3;
        }

        public void XLocalMove(float n) => LocalMove(n, transformComponent.localPosition.y, transformComponent.localPosition.z);
        public void YLocalMove(float n) => LocalMove(transformComponent.localPosition.x, n, transformComponent.localPosition.z);
        public void ZLocalMove(float n) => LocalMove(transformComponent.localPosition.x, transformComponent.localPosition.y, n);


        //Rotate ------------------------------------------------------------------

        //Color ------------------------------------------------------------------

        public void LerpFunction(UnityEvent function, float time)
        { 

        }

        public IEnumerator LerpFunctionCoroutine(UnityEvent function, float time)
        {
            yield return null;
        }
    }
}