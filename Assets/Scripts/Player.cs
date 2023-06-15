using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]

public class Player : NetworkBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpDelay = 1.05f;
    [SerializeField] private bool isJumped = false;
    [SerializeField] private Bullet[] bulletPrefabs;
    [SerializeField] private Transform shootPoint;
    private Renderer _renderer;
  
    [SerializeField] private NetworkVariable<Vector2> input = new(
        Vector2.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    [SerializeField] private NetworkVariable<bool> jump = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    /*[SerializeField] private NetworkVariable<bool> shoot = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);*/
    [SerializeField] private NetworkVariable<Color> networkColor = new(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
    }

    [ServerRpc]
    private void ShootServerRpc()
    {
        var randomBullet = Random.Range(0, bulletPrefabs.Length);
        var bullet = Instantiate(bulletPrefabs[randomBullet], shootPoint.position, Quaternion.identity);
        bullet.Spawn(shootPoint.forward);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            networkColor.Value = Random.ColorHSV();
        }
    }
    
    private void Update()
    {
        _renderer.material.color = networkColor.Value;
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                networkColor.Value = Random.ColorHSV();
            }
            input.Value = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"));
            jump.Value = Input.GetKey(KeyCode.Space);
            if (Input.GetMouseButtonDown(0))
            {
                ShootServerRpc();
            }
        }

        if (IsServer)
        {
            var direction = new Vector3(input.Value.x, 0, input.Value.y);
            _rigidbody.AddForce(direction * (speed * Time.deltaTime), ForceMode.Impulse);
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            if (jump.Value && !isJumped)
            {
                StartCoroutine(JumpDelay());
                _rigidbody.AddForce(Vector3.up* jumpPower,ForceMode.Impulse);
            }
        }
    }

    private IEnumerator JumpDelay()
    {
        isJumped = true;
        yield return new WaitForSeconds(jumpDelay);
        isJumped = false;

    }
}
