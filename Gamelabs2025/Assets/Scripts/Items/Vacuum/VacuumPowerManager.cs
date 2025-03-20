using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Items
{
    public class VacuumPowerManager : MonoBehaviour
    {
        private static float power = -1f;

        private float maxPower;
        private Coroutine rechargeRoutine;
        private float rechargeRate;
        private float useRate;
        private bool vacuumOn = false;

        public event Action OnPowerDepleted;
        
		private static VacuumPowerManager instance;
        public static VacuumPowerManager Instance
        {
            get
            {
                if(instance == null)
                    instance = new GameObject("VacuumPowerManager").AddComponent<VacuumPowerManager>();
                return instance;
            }
        }

        public bool HasPower => power >= 1;
        public float PowerPercentage => power / maxPower;
        
        public void Initialise(float maxPower, float useRatePerSec, float rechargeRatePerSec)
        {
            //only initialise when power was not set initially for the first time
            if(power < 0)
                power = maxPower;
            
            this.maxPower = maxPower;
            this.useRate = useRatePerSec;
            this.rechargeRate = rechargeRatePerSec;
        }

        public void SetVacuumActiveStatus(bool vacuumOn)
        {
            this.vacuumOn = vacuumOn;
        }
        
        private void Update()
        {
            if (vacuumOn)
            {
                if (rechargeRoutine != null)
                {
                    StopCoroutine(rechargeRoutine);
                    rechargeRoutine = null;
                }

                power -= useRate * Time.deltaTime;
                if (power <= 0)
                {
                    OnPowerDepleted?.Invoke();
                    Recharge();
                }
            }
            else
            {
                if(power < maxPower && rechargeRoutine == null)
                    Recharge();
            }
        }

        private void Recharge()
        {
            if(rechargeRoutine != null)
                StopCoroutine(rechargeRoutine);

            rechargeRoutine = StartCoroutine(RechargeRoutine());
        }

        IEnumerator RechargeRoutine()
        {
            yield return new WaitForSeconds(1.5f);
            while (power < maxPower)
            {
                power = Mathf.Clamp(power + rechargeRate * Time.deltaTime, 0, maxPower);
                yield return new WaitForEndOfFrame();
            }

            rechargeRoutine = null;
        }
    }
}