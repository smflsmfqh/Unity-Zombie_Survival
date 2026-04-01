using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

// --- 좀비 스포너 클래스 ---
// SpawnPoints Prefab의 4개 스폰 포인트 만들어져 있음
// 해당 포인트에 랜덤으로 좀비 생성하며, 웨이브가 오를수록 생성 수가 증가함

public class ZombieSpawner : MonoBehaviour
{
    public GameManager gameManager;
    public UIManager uiManager;
    public Zombie prefab; // 생성할 좀비 프리팹

    public ZombieData[] zombieDatas; // 좀비 유형 데이터 배열 (랜덤 적용됨)
    public Transform[] spawnPoints; // 좀비가 등장할 수 있는 스폰 포인트 목록

    private List<Zombie> zombies = new List<Zombie>(); // 현재 살아있는 좀비 목록
    
    private int wave; // 현재 웨이브 번호 (회차), 좀비의 생성 수는 wave * 1.5f로 결정됨

    private void Update()
    {
        // 매 프레임 살아있는 좀비가 없다면 다음 웨이브를 시작
        if (zombies.Count == 0)
        {
            SpawnWave();
        }
    }

    // --- 웨이브 증가 후, wave * 1.5f(반올림)만큼 좀비를 생성하고 UI 갱신 ---
    private void SpawnWave()
    {
        wave++;
        int count = Mathf.RoundToInt(wave * 1.5f); // 웨이브가 오를 수록 생성 수가 증가됨

        for (int i = 0; i < count; i++)
        {
            CreateZombie();
        }

        uiManager.SetWaveText(wave, zombies.Count);

    }

    // --- 랜덤 스폰 포인트에 랜덤 유형의 좀비를 생성하고 이벤트 등록 메서드 ---
    private void CreateZombie()
    {
        var point = spawnPoints[Random.Range(0, spawnPoints.Length)]; // 스폰 포인트 중 하나를 랜덤 선택
        var zombie = Instantiate(prefab, point.position, point.rotation); // 좀비 프리팹을 point의 위치와 방향에 인스턴스화
        zombie.Setup(zombieDatas[Random.Range(0, zombieDatas.Length)]); // 좀비 데이터 배열에서 랜덤으로 유형을 선택해 초기 설정
        zombies.Add(zombie); // 살아있는 좀비 목록에 추가

        // 좀비 사망 시 처리: 목록에서 제거 -> UI 갱신 -> 5초 후 오브젝트 삭제
        zombie.onDead.AddListener(() => zombies.Remove(zombie));
        zombie.onDead.AddListener(() => gameManager.AddScore(100));    
        zombie.onDead.AddListener(() => uiManager.SetWaveText(wave, zombies.Count));
        zombie.onDead.AddListener(() => Destroy(zombie.gameObject, 5f));
    }


}
