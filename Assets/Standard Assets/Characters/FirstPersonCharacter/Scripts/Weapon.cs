using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Weapons : MonoBehaviour
{

    //Weapon
    public bool _Automatic = true;
    public int _AmmosLoad = 30;
    public float _ReloadTime = 2f;
    public float _RecoveryTime = 0.5f;
    public float _WeaponRange = 100f;
    private int _CurrentAmmo;
    
    public int _GetCurrentAmmo
    {
        get { return _CurrentAmmo; }
    }
   

    private bool _IsReloading = false;
    private float _LastShot = 0f;

    private void Start()
    {
        _CurrentAmmo = _AmmosLoad;
        //_WeaponShotSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        _LastShot -= Time.deltaTime;
        _LastShot = Mathf.Clamp(_LastShot, 0, _RecoveryTime);
    }

    public void Shoot(Vector3 p_Position, Vector3 p_Direction)
    {

        if (_CurrentAmmo <= 0 || _IsReloading || _LastShot > 0)
        {
            return;
        }

        _LastShot = _RecoveryTime;
        --_CurrentAmmo;

        RaycastHit v_Hit;

        Debug.DrawRay(p_Position, p_Direction * _WeaponRange, Color.red, 10f);

        if (Physics.Raycast(p_Position, p_Direction, out v_Hit, _WeaponRange))
        {

            GameObject v_Target = v_Hit.collider.gameObject;

            if (v_Target != null)
            {
                Debug.Log("Shoot:" + v_Target.name);
                /*
                v_Target.TakeDamage(_Statistics._Damages);
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Renderer rnd = obj.GetComponent<Renderer>();
                rnd.material.color = Color.red;
                obj.transform.position = v_Hit.point;*/
            }
            else
            {
                Debug.Log("Shoot: Autre");

                /*GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Renderer rnd = obj.GetComponent<Renderer>();
                rnd.material.color = Color.yellow;
                obj.transform.position = v_Hit.point;*/

            }
        }
    }

    public void Reload()
    {
        if (_CurrentAmmo >= _AmmosLoad || _IsReloading)
        {
            return;
        }

        Debug.Log("Reload");

        _IsReloading = true;
        StartCoroutine(DoReloading());
    }

    public IEnumerator DoReloading()
    {
        yield return new WaitForSeconds(_ReloadTime);
        _CurrentAmmo = _AmmosLoad;
        _IsReloading = false;
    }
}
