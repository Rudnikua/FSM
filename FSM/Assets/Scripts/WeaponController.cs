using StarterAssets;
using UnityEngine;

public class WeaponController : MonoBehaviour {

    [SerializeField] private WeaponSO currentWeapon;

    private EquipmentSystem equipmentSystem;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    private BoxCollider longSwordCollider;

    private const string DRAW_SHEATH_WEAPON = "DrawWeapon";
    private const string ATTACK = "Attack";

    private bool canAttack = false;
    private bool isDrawn = false;
    private bool inAction = false;

    private void Awake() {
        equipmentSystem = GetComponent<EquipmentSystem>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
    }

    private void Update() {
        DrawOrSheathWeapon();
        OnAttack();

        starterAssetsInputs.draw = false;
        starterAssetsInputs.attack = false;
    }

    private void DrawOrSheathWeapon() {
        if (inAction) {
            return;
        }
        if (starterAssetsInputs.draw) {
            if (!isDrawn) {
                animator.SetBool(DRAW_SHEATH_WEAPON, true);
                isDrawn = true;
                inAction = false;
            } else {
                animator.SetBool(DRAW_SHEATH_WEAPON, true);
                isDrawn = false;
                inAction = true;
                canAttack = false;
            }
        }
    }


    private void OnAttack() {
        if (starterAssetsInputs.attack && canAttack) {
            animator.SetBool(ATTACK, true);
        }
    }

    private void OnDrawFinished() {
        animator.SetBool(DRAW_SHEATH_WEAPON, false);
        inAction = false;
        canAttack = true;
    }
    private void OnSheathFinished() {
        animator.SetBool(DRAW_SHEATH_WEAPON, false);
        inAction = false;
    }

    private void OnAttackFinished() {
        starterAssetsInputs.attack = false;
        animator.SetBool(ATTACK, false);
    }
    public void EnableSwordCollider() {
        if (equipmentSystem.GetCurrentWeaponInHand() != null) {
            equipmentSystem.GetCurrentWeaponInHand().GetComponent<BoxCollider>().enabled = true;
            Debug.Log("Collider is enabled");
        }
    }

    public void DisableSwordCollider() {
        if (equipmentSystem.GetCurrentWeaponInHand() != null) {
            equipmentSystem.GetCurrentWeaponInHand().GetComponent<BoxCollider>().enabled = false;
            Debug.Log("Collider is disabled");
        }
    }
}
