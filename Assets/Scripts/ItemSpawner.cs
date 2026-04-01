using UnityEngine;
using UnityEngine.AI;

// --- Item 스포너 클래스 ---
// NavMesh 위의 랜덤한 위치에 일정 간격으로 아이템을 생성하고, 일정 시간 후 자동 삭제
public class ItemSpawner : MonoBehaviour
{
    public GameObject[] items; // item 프리팹 배열

    public float maxDistance = 10f; // 스폰 기준점으로부터 탐색할 최대 반경
    public float interval = 0.5f; // 아이템 생성 간격 (초)
    public float itemDuration = 20f; // 아이템이 자동 삭제되기까지의 시간 (초)

    private float spawnTimer = 0f; // 마지막 생성 이후 경과 시간 누적

  
    // --- interval마다 Spawn() 호출 ---
    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer > interval)
        {
            spawnTimer = 0f;
            Spawn();
        }
    }

    // --- NavMesh 위의 랜덤 위치에 랜덤 아이템 생성 ---
    private void Spawn()
    {
        // 구체 범위 안의 랜덤 위치 계산
        var randomPos = Random.insideUnitSphere * maxDistance;

        // 해당 위치 근처에 유효한 NavMesh가 없으면 생성 취소
        if (!NavMesh.SamplePosition(randomPos, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            return;
        }

        // NavMesh 위의 좌표로 보정 후 살짝 띄워 지면 관통 방지
        randomPos = hit.position;
        randomPos.y += 0.5f;

        // 랜덤 아이템 프리팹을 생성하고 itemDuration 후 자동 삭제 
        var newGo = Instantiate(items[Random.Range(0, items.Length)], randomPos, Quaternion.identity);

        Destroy(newGo, itemDuration);
    }

  
}
