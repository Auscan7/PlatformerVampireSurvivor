using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CharacterMovement characterMovement;

    protected virtual void Awake()
    {
        DontDestroyOnLoad(this);

        rb = GetComponent<Rigidbody2D>();
        characterMovement = GetComponent<CharacterMovement>();
    }

    protected virtual void Update()
    {

    }
}
