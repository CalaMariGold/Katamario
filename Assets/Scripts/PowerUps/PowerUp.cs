using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PowerUp : MonoBehaviour
{
    private Animator _animator;
    private MeshRenderer _meshRenderer;
    private CapsuleCollider _capsuleCollider;
    [SerializeField] private GameObject _particles;
    [SerializeField] private TMP_Text _text;

    private float cooldown = 5;

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();
        _meshRenderer = this.GetComponent<MeshRenderer>();
        _capsuleCollider = this.GetComponent<CapsuleCollider>();
    }

    void Start()
    {
        _capsuleCollider.enabled = false;
        _meshRenderer.material.color = Color.gray;
        _animator.enabled = false;
        _text.text = cooldown.ToString();
        _particles.SetActive(false);

        StartCoroutine(PowerUpCoolDown((int)cooldown));
    }

    void Update()
    {
        cooldown -= Time.deltaTime;
        _text.text = ((int)cooldown).ToString();
    }

    private IEnumerator PowerUpCoolDown(int cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        EnablePowerUp();

        yield return null;
    }

    private void EnablePowerUp()
    {
        _capsuleCollider.enabled = true;
        _animator.enabled = true;
        _meshRenderer.material.color = Color.blue;
        _particles.SetActive(true);
        _text.enabled = false;
    }
}
