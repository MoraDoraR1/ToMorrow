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
    private bool spawningActive = true;

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

        // 현재 스테이지의 몬스터 종류 중 랜덤으로 골라 이미지/스탯 적용
        StageInfo info = StageManager.Instance != null ? StageManager.Instance.CurrentStageInfo : null;
        if (info != null && info.monsters != null && info.monsters.Length > 0)
        {
            MonsterVariant v = info.monsters[Random.Range(0, info.monsters.Length)];
            if (v != null) currentMonster.Initialize(v.hp, v.coinReward, v.sprite);
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
        // 중복 호출 방지를 위해 구독 해제 후, 스폰이 활성 상태일 때만 다음 몬스터 스폰
        dead.OnDeath -= HandleMonsterDeath;
        if (spawningActive)
        {
            SpawnMonster();
        }
    }

    /// <summary>일반 몬스터 스폰을 멈추고, 현재 몬스터를 제거한다. (보스전 진입 시)</summary>
    public void StopAndClear()
    {
        spawningActive = false;
        if (currentMonster != null)
        {
            currentMonster.OnDeath -= HandleMonsterDeath;
            currentMonster.gameObject.SetActive(false); // UI 잔상 방지
            Destroy(currentMonster.gameObject);
            currentMonster = null;
        }
    }

    /// <summary>일반 몬스터 스폰을 재개하고 새 몬스터를 배치한다. (보스전 종료 시)</summary>
    public void ResumeSpawning()
    {
        spawningActive = true;
        SpawnMonster();
    }
}
