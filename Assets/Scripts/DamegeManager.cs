using UnityEngine;
using UnityEngine.UI;

public class DamegeManager : MonoBehaviour
{
    //제 개인적인 취향으로 함수는 파스칼, 변수명은 카멜로 표현했습니다. 요건 바꾸셔도 문제 없습니다.

    //어떤 버튼이 클리커 인지 넣을 자리 마련
    public Button UserClicker;

    //유저 공격력 UI 자리 마련
    public Text UserAtk;

    //유저가 잡은 몬스터 수 어떤 텍스트에 표시할 건지 자리 마련
    public Text StackKill;

    //몬스터 체력 UI 자리 마련
    public Text EnemyHp;

    //몬스터 종류 UI 자리 마련
    public Text EnemyValue;

    // 몬스터 이미지
    public Image EnemyImage;

    // 일반 몬스터 이미지
    public Sprite NormalSprite;

    // 보스 몬스터 이미지
    public Sprite BossSprite;



    // 유저 공격력 수치 변수
    // 임의로 50으로 잡았습니다
    private int userAtk = 50;

    // 잡은 몬스터 수치 변수
    private int stackKill = 0;

    // 몬스터 HP 수치 변수
    // 임의로 일반몹 -> 200, 보스몹 -> 1000으로 수치 잡고 짰습니다
    private int enemyHp = 200;
    private int enemyBossHp = 1000;

    // 몬스터 등급 종류 
    // 임의로 일반, 보스 2가지로 정리했습니다.
    private string[] enemyValue = new string[] { "Normal", "Boss" };


    void Start()
    {
        // 클리커 버튼이 눌리면 ()안의 함수 작동 -> 이게 KillMonster 함수 작동 시킴
        UserClicker.onClick.AddListener(KillMonster);
    }

    void Update()
    {
        
    }

    // 버튼 눌렀을 때 몬스터 한테 데미지 주고 UI에도 반영하는 함수
    // private -> 이 스크립에서만 사용
    // void -> 암거나 때려박아도 되는 자료형
    // KillMonster() -> 함수 이름, 바꿔서 사용하셔도 무관합니다
    private void KillMonster()
    {
        // 유저 공격력 수치 만큼 몬스터 체력 감소
        enemyHp -= userAtk;

        // 체력이 -로 내려가는 것 방지를 위한 조건문
        if (enemyHp > 0)
        {
            // 감소된 체력 UI에 출력
            EnemyHp.text = enemyHp.ToString();
        }
        else
        {
            // 몬스터 체력이 0이하로 변해서 킬 스택 상승
            stackKill++;

            // 킬 스택 오른거 UI에 적용
            StackKill.text = stackKill.ToString();

            // 5의 배수마다 보스 등장
            // 임의로 5의 배수로 줄였습니다. 프로토타입인데 50번까지 해보라는 악독한 행동은 참아주십쇼
            if (stackKill % 5 == 0)
            {
                // 체력 수치를 보스 체력으로 변경
                enemyHp = enemyBossHp;

                // UI에 변경 수치 반영
                EnemyValue.text = enemyValue[1];

                // 이미지를 보스 이미지로 변경
                EnemyImage.sprite = BossSprite;
            }
            else
            {
                // 체력 수치 일반몹 유지
                enemyHp = 200;

                // UI에 변경 수치 반영
                EnemyValue.text = enemyValue[0];

                // 이미지 일반몹 이미지로 변경
                EnemyImage.sprite = NormalSprite;
            }

            // 바뀐 체력 UI에 출력
            EnemyHp.text = enemyHp.ToString();
        }
    }
}
