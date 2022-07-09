using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speedMove = 5f;
    [SerializeField] private float _gravityForce = 20f;
    [SerializeField] private Animator _animator;

    [SerializeField] private GameObject _sword;
    [SerializeField] private GameObject _automatic;

    [Tooltip("Слой Ground, нужен для определения направления взгляда")]
    [SerializeField] private LayerMask _raycastLayerMask = (1 << 6);

    [SerializeField] private Enemy _enemy;

    [Tooltip("Расстояние в пределах которого можно добить врага")]
    [SerializeField] private float _minDistToFinishing = 3f;

    [Tooltip("Расстояние добивания")]
    [SerializeField] private float _finishingDistance = 1f;

    [Tooltip("Сообщение при приближении к врагу")]
    [SerializeField] private GameObject _message;

    [Header("Upper body")]
    [Tooltip("Верняя часть тела")]
    [SerializeField] private Transform _upperBodyBone;

    [Tooltip("Смещение угла поворота верхней части тела")]
    [SerializeField] private float _upperBodyRotationOfset = 80f;

    [Tooltip("Угол до мыши при которолм приосходит поворот")]
    [SerializeField] private float _maxRotationAngle= 70f;

    [Tooltip("Скорость поворота на месте")]
    [SerializeField] private float _rotationSpeed = 5f;

    private CharacterController _characterController;
    private Vector3 _anchorPosition;
    private Vector2 _inputValue;
    private float _currentGravityForce;

    private bool _killing = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        _inputValue = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        if (_killing) return;

        Move();
        Gravity();
        Rotation();
        EnemyDetection();
    }

    private void LateUpdate()
    {
        if (_killing) return;
        UpperBodyRotation();
    }

    private void Move()
    {
        Vector3 moveVector = Vector3.zero;

        moveVector.x = _inputValue.x * _speedMove;
        moveVector.z = _inputValue.y * _speedMove;
        moveVector.y = _currentGravityForce;

        _characterController.Move(moveVector * Time.deltaTime);

        UpdateMovementAnimations();
    }

    private void Rotation()
    {
        UpdateAnchorPosition();

        if (_inputValue != Vector2.zero)
        {
            transform.LookAt(_anchorPosition);
        }
        else
        {
            Vector3 direction = _anchorPosition - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);

            if(Mathf.Abs(angle) > _maxRotationAngle)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation,rotation, _rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void UpperBodyRotation()
    {
        Vector3 rotateTo = _anchorPosition - _upperBodyBone.position;
        float angle = Mathf.Atan2(rotateTo.x, rotateTo.z) * Mathf.Rad2Deg;
        _upperBodyBone.rotation = Quaternion.Euler(_upperBodyBone.eulerAngles.x, angle - _upperBodyRotationOfset, _upperBodyBone.eulerAngles.z);
    }

    private void EnemyDetection()
    {
        if(Vector3.Distance(transform.position,_enemy.transform.position) < _minDistToFinishing && !_enemy.isDead)
        {
            _message.SetActive(true);

            if (!_killing && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Finishing());
            }
        }
        else
        {
            _message.SetActive(false);
        }
    }

    private IEnumerator Finishing()
    {
        _killing = true;
        _animator.SetTrigger("Finishing");
        MoveToEnemy();
        ChangeWeapon(true);
        _enemy.Hit();

        yield return new WaitForSeconds(1.35f);
        
        ChangeWeapon(false);
        _killing = false;
    }

    private void ChangeWeapon(bool sword)
    {
        _automatic.SetActive(!sword);
        _sword.SetActive(sword);
    }

    private void MoveToEnemy()
    {
        float distToEnemy = Vector3.Distance(transform.position, _enemy.transform.position);
        float procent = (_finishingDistance * 100 / distToEnemy)/ 100;

        transform.position = Vector3.Lerp(transform.position, _enemy.transform.position,1 - procent);
        transform.rotation =  Quaternion.LookRotation(_enemy.transform.position - transform.position);
    }

    private void UpdateAnchorPosition()
    {
        Vector2 inputMousePosition = Input.mousePosition;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(inputMousePosition);

        if (Physics.Raycast(ray, out hit, 100, _raycastLayerMask))
        {
            Vector3 hitPositon = hit.point;
            _anchorPosition = hitPositon;
        }

        _anchorPosition.y = transform.position.y;
    }

    private void Gravity()
    {
        if (!_characterController.isGrounded)
        {
            _currentGravityForce -= _gravityForce * Time.deltaTime;
        }
        else
        {
            _currentGravityForce = -1;
        }
    }

    private void UpdateMovementAnimations()
    {
        float rotationY = transform.localRotation.eulerAngles.y;
        Vector3 moveDirection = Quaternion.Euler(0, -rotationY, 0) * new Vector3(_inputValue.x, 0, _inputValue.y);

        _animator.SetFloat("Horisontal", moveDirection.x);
        _animator.SetFloat("Vertical", moveDirection.z);
    }
}
