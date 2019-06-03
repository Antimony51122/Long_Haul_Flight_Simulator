using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour {
    private Animator _animator = null;

    [SerializeField] private Avatar _avatarSit      = null;
    [SerializeField] private Avatar _avatarTraverse = null;

    [SerializeField] private RuntimeAnimatorController _controllerSit      = null;
    [SerializeField] private RuntimeAnimatorController _controllerTraverse = null;

    [SerializeField] private bool _isTraversing;

    private NavAgentNoRootMotion _navAgentNoRootMotion;

    void Start() {
        _animator = GetComponent<Animator>();

        _isTraversing = false;

        _navAgentNoRootMotion = GetComponent<NavAgentNoRootMotion>();

        _navAgentNoRootMotion.enabled = false;
    }

    void Update() {
        AssignTraverseAnimator();
        AssignSitAnimator();
    }

    private void AssignTraverseAnimator() {
        if (_isTraversing) {
            _animator.runtimeAnimatorController = _controllerTraverse;
            _animator.avatar                    = _avatarTraverse;

            _navAgentNoRootMotion.enabled = true;
        }
    }

    private void AssignSitAnimator() {
        if (!_isTraversing) {
            _animator.runtimeAnimatorController = _controllerSit;
            _animator.avatar                    = _avatarSit;

            _navAgentNoRootMotion.enabled = false;
        }
    }
}