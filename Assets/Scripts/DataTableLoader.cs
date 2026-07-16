using UnityEngine;

/// <summary>
/// Resources/Data/ 의 CSV들을 읽어 각 매니저의 "수치"를 덮어쓴다.
/// 스프라이트와 배열 크기는 인스펙터가 소유하며, CSV는 기존 항목의 숫자만 갱신한다.
///
/// 모든 Awake는 Start보다 먼저 실행되므로, 여기서 주입한 값은
/// 스포너/보스 등이 Start에서 사용할 때 이미 반영되어 있다.
/// 엑셀 수정 → CSV 저장 → Play 하면 새 수치가 적용된다.
/// </summary>
public class DataTableLoader : MonoBehaviour
{
    [Header("적용 대상 (Instance 대신 직접 참조 — Awake 순서 문제 방지)")]
    public StageManager stageManager;
    public CharacterManager characterManager;
    public StoryManager storyManager;
    public ConstellationManager constellationManager;
    public FullMoonAttack fullMoonAttack;
    public BossManager bossManager;
    public OfflineReward offlineReward;

    [Header("CSV 경로 (Resources 아래, 확장자 제외)")]
    public string stageCsv = "Data/Stage";
    public string monsterCsv = "Data/Monster";
    public string characterCsv = "Data/Character";
    public string storyCsv = "Data/Story";
    public string constellationCsv = "Data/Constellation";
    public string balanceCsv = "Data/Balance";

    void Awake()
    {
        LoadStages();
        LoadMonsters();
        LoadCharacters();
        LoadStories();
        LoadConstellations();
        LoadBalance();
    }

    // Stage.csv : stage, bossHp, bossCoinReward
    void LoadStages()
    {
        if (stageManager == null) return;
        CsvTable t = CsvTable.Load(stageCsv, "stage");
        if (t == null) return;

        for (int r = 0; r < t.RowCount; r++)
        {
            int stage = t.GetInt(r, "stage", -1);
            StageInfo info = stageManager.GetStageInfo(stage);
            if (info == null || info.boss == null)
            {
                Debug.LogWarning("DataTableLoader: Stage " + stage + " 가 인스펙터에 없습니다. (건너뜀)");
                continue;
            }

            info.boss.hp = t.GetInt(r, "bossHp", info.boss.hp);
            info.boss.coinReward = t.GetInt(r, "bossCoinReward", info.boss.coinReward);
        }
    }

    // Monster.csv : stage, index, hp, coinReward
    void LoadMonsters()
    {
        if (stageManager == null) return;
        CsvTable t = CsvTable.Load(monsterCsv, "stage");
        if (t == null) return;

        for (int r = 0; r < t.RowCount; r++)
        {
            int stage = t.GetInt(r, "stage", -1);
            int index = t.GetInt(r, "index", -1);

            StageInfo info = stageManager.GetStageInfo(stage);
            if (info == null || info.monsters == null || index < 0 || index >= info.monsters.Length)
            {
                Debug.LogWarning("DataTableLoader: Stage " + stage + " 의 monster[" + index + "] 가 인스펙터에 없습니다. (건너뜀)");
                continue;
            }

            MonsterVariant v = info.monsters[index];
            if (v == null) continue;
            v.hp = t.GetInt(r, "hp", v.hp);
            v.coinReward = t.GetInt(r, "coinReward", v.coinReward);
        }
    }

    // Character.csv : index, unlockCost, baseDamage, damagePerLevel, maxLevel, levelUpBaseCost, levelUpCostPerLevel
    void LoadCharacters()
    {
        if (characterManager == null) return;
        CsvTable t = CsvTable.Load(characterCsv, "index");
        if (t == null) return;

        for (int r = 0; r < t.RowCount; r++)
        {
            int index = t.GetInt(r, "index", -1);
            CharacterData c = characterManager.Get(index);
            if (c == null)
            {
                Debug.LogWarning("DataTableLoader: Character[" + index + "] 가 인스펙터에 없습니다. (건너뜀)");
                continue;
            }

            c.unlockCost = t.GetInt(r, "unlockCost", c.unlockCost);
            c.baseDamage = t.GetInt(r, "baseDamage", c.baseDamage);
            c.damagePerLevel = t.GetInt(r, "damagePerLevel", c.damagePerLevel);
            c.maxLevel = t.GetInt(r, "maxLevel", c.maxLevel);
            c.levelUpBaseCost = t.GetInt(r, "levelUpBaseCost", c.levelUpBaseCost);
            c.levelUpCostPerLevel = t.GetInt(r, "levelUpCostPerLevel", c.levelUpCostPerLevel);
        }
    }

    // Story.csv : index, characterIndex, requiredLevel, cost, moonDamageBonus, moonIntervalReduction
    void LoadStories()
    {
        if (storyManager == null) return;
        CsvTable t = CsvTable.Load(storyCsv, "index");
        if (t == null) return;

        for (int r = 0; r < t.RowCount; r++)
        {
            int index = t.GetInt(r, "index", -1);
            StoryData s = storyManager.Get(index);
            if (s == null)
            {
                Debug.LogWarning("DataTableLoader: Story[" + index + "] 가 인스펙터에 없습니다. (건너뜀)");
                continue;
            }

            s.characterIndex = t.GetInt(r, "characterIndex", s.characterIndex);
            s.requiredLevel = t.GetInt(r, "requiredLevel", s.requiredLevel);
            s.cost = t.GetInt(r, "cost", s.cost);
            s.moonDamageBonus = t.GetInt(r, "moonDamageBonus", s.moonDamageBonus);
            s.moonIntervalReduction = t.GetFloat(r, "moonIntervalReduction", s.moonIntervalReduction);
        }
    }

    // Constellation.csv : index, starCount, unlockStage, starBaseCost, starCostIncrease, effectType, effectValue
    void LoadConstellations()
    {
        if (constellationManager == null) return;
        CsvTable t = CsvTable.Load(constellationCsv, "index");
        if (t == null) return;

        for (int r = 0; r < t.RowCount; r++)
        {
            int index = t.GetInt(r, "index", -1);
            ConstellationData c = constellationManager.Get(index);
            if (c == null)
            {
                Debug.LogWarning("DataTableLoader: Constellation[" + index + "] 가 인스펙터에 없습니다. (건너뜀)");
                continue;
            }

            c.starCount = t.GetInt(r, "starCount", c.starCount);
            c.unlockStage = t.GetInt(r, "unlockStage", c.unlockStage);
            c.starBaseCost = t.GetInt(r, "starBaseCost", c.starBaseCost);
            c.starCostIncrease = t.GetInt(r, "starCostIncrease", c.starCostIncrease);
            c.effectType = (ConstellationEffectType)t.GetInt(r, "effectType", (int)c.effectType);
            c.effectValue = t.GetFloat(r, "effectValue", c.effectValue);
        }
    }

    // Balance.csv : key, value  (moonDamage, moonInterval, bossDuration, killsRequired)
    void LoadBalance()
    {
        CsvTable t = CsvTable.Load(balanceCsv, "key");
        if (t == null) return;

        for (int r = 0; r < t.RowCount; r++)
        {
            string key = t.GetString(r, "key");
            if (string.IsNullOrEmpty(key)) continue;

            switch (key)
            {
                case "moonDamage":
                    if (fullMoonAttack != null) fullMoonAttack.damage = t.GetInt(r, "value", fullMoonAttack.damage);
                    break;
                case "moonInterval":
                    if (fullMoonAttack != null) fullMoonAttack.interval = t.GetFloat(r, "value", fullMoonAttack.interval);
                    break;
                case "bossDuration":
                    if (bossManager != null) bossManager.bossDuration = t.GetFloat(r, "value", bossManager.bossDuration);
                    break;
                case "killsRequired":
                    if (stageManager != null) stageManager.SetKillsRequired(t.GetInt(r, "value", stageManager.KillsRequired));
                    break;
                case "offlineRate":
                    if (offlineReward != null) offlineReward.rate = t.GetFloat(r, "value", offlineReward.rate);
                    break;
                case "offlineMaxHours":
                    if (offlineReward != null) offlineReward.maxHours = t.GetFloat(r, "value", offlineReward.maxHours);
                    break;
                default:
                    Debug.LogWarning("DataTableLoader: Balance.csv 에 모르는 key → " + key);
                    break;
            }
        }
    }
}
