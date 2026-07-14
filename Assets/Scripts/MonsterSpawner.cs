using UnityEngine;

/// <summary>
/// 인게임에 몬스터를 1마리씩 스폰하고, 처치되면 즉시 다음 몬스터를 배치한다.
/// 새로 스폰된 몬스터를 PlayerAttack의 현재 타겟으로 갱신한다.
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰할 몬스터 프리팹 (RectTransform + EnemyHealth 포함)")]
    public GameObject monsterPrefab;

    [Header("몬스터가 생성될 부모 (보통 Canvas)")]
    public Transform spawnParent;

    [Header("스폰 위치 (선택 - 비우면 프리팹 자체 위치 사용)")]
    public RectTransform spawnPoint;

    [Header("타겟을 갱신할 PlayerAttack")]
    public PlayerAttack playerAttack;

    private EnemyHealth currentMonster;

    void Start()
    {
        SpawnMonster();
    }

    void SpawnMonster()
    {
        if (monsterPrefab == null || spawnParent == null)
        {
            Debug.LogWarning("MonsterSpawner: 필수 참조(몬스터 프리팹/부모)가 비어 있습니다.");
            return;
        }

        GameObject monsterObj = Instantiate(monsterPrefab, spawnParent);

        RectTransform monsterRect = monsterObj.GetComponent<RectTransform>();
        // 스폰 위치가 지정된 경우에만 위치를 맞춤 (아니면 프리팹에 저장된 위치 사용)
        if (monsterRect != null && spawnPoint != null)
        {
            monsterRect.position = spawnPoint.position;
        }

        currentMonster = monsterObj.GetComponent<EnemyHealth>();
        if (currentMonster == null)
        {
            Debug.LogError("MonsterSpawner: 몬스터 프리팹에 EnemyHealth가 없습니다.");
            return;
        }

        // 이 몬스터가 죽으면 다음 몬스터를 스폰하도록 구독
        currentMonster.OnDeath += HandleMonsterDeath;

        // 새 몬스터를 플레이어의 공격 타겟으로 지정
        if (playerAttack != null && monsterRect != null)
        {
            playerAttack.SetTarget(monsterRect);
        }
    }

    void HandleMonsterDeath(EnemyHealth dead)
    {
        // 중복 호출 방지를 위해 구독 해제 후 다음 몬스터 스폰
        dead.OnDeath -= HandleMonsterDeath;
        SpawnMonster();
    }
}
