using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_AlertSearching : MonoBehaviour
{
    NavMeshAgent agent;
    enemy_stage enemy_script;
    public enum SearchType {serach_building, keepposition }
    public SearchType currentSearch;
    [Header("AlertSearch Settings")]
    bool isAlertRoutineRunning = false;
    private Coroutine mainSearchCoroutine; // สร้างตัวแปรมาเก็บ Coroutine หลัก

    [Header("Building Search Settings")]
    public float scanRadius = 50f;          // รัศมีในการหาบ้านรอบตัว
    public LayerMask waypointLayer;         // Layer ของตัวบ้านหรือจุด Waypoint หลักของบ้าน
    public float Search_inside_radius = 10;
    public int MaxSearchPoint = 4;
    private int currentSearchpoint = 0;
    private List<Vector3> buildingsToSearch = new List<Vector3>();
    private int currentBuildingIndex = 0;
   
    private int insideHouseBit;

    [Header("Keep Position Settings")]
    public Vector3 CurrentPosition;       // พิกัดล่าสุดที่ได้รับแจ้งมา (เช่น จุดเสียงดัง หรือจุดเจอผู้เล่น)
    public float searchRadiusAroundPoint = 12f; // รัศมีขอบเขตในการเดินตรวจ (ห้ามหลุดจากนี้)
    

    [Header("Keep Position Settings")]
    public Vector3 susPoint;
    
    [Header("Relocated Settings")]
    public bool isRelocate = false;
   
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy_script = GetComponent<enemy_stage>();
        waypointLayer = LayerMask.GetMask("Bwaypoint");

        // เก็บหน้ากากพื้นที่ไว้ตั้งแต่เริ่มเกม
        int index = NavMesh.GetAreaFromName("InsideHouse");
        insideHouseBit = (1 << index);
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy_script.currentState == enemy_stage.EnemyState.alertSearching)
        {
            if (!isAlertRoutineRunning)
            {
                isAlertRoutineRunning = true;
                // เก็บค่า Coroutine ลงตัวแปร
                mainSearchCoroutine = StartCoroutine(AlertSearchRoutine());
            }
        }
        else
        {
            if (isAlertRoutineRunning)
            {
                // สั่งหยุดเฉพาะเป้าหมายที่กำลังรันอยู่
                if (mainSearchCoroutine != null)
                {
                    StopCoroutine(mainSearchCoroutine);
                    mainSearchCoroutine = null;
                }

                isAlertRoutineRunning = false;
                //agent.ResetPath();
            }
        }
    }

    IEnumerator AlertSearchRoutine()
    {
        //yield return StartCoroutine(SurroundPlayerRoutine());

        random_Task();

        while (enemy_script.currentState == enemy_stage.EnemyState.alertSearching)
        {
            if (currentSearch == SearchType.serach_building)
            {
                yield return StartCoroutine(SearchBuilding_Sequence());
            }

            else
            {
               yield return  StartCoroutine(KeepPosition_Sequence());
            }
        }

    }

    public void GetSuspicious_pos(Vector3 SPoint) //เดินเช็กจุดที่น่าสงสัยหรือจุดที่เห็นผู้เล่นล่าสุด
    {
        susPoint = SPoint;
    }

    void scan_Building() //เดินเช็กตึก/บ้าน
    {
        buildingsToSearch.Clear();
        currentBuildingIndex = 0;

        //ใช้overlapShereในการแสกนหาตึกรอบตัว โดยบ้านแต่ละหลังจะมีwayPointของมันเอง
        Collider[] building = Physics.OverlapSphere(transform.position, scanRadius, waypointLayer);

        foreach (Collider b in building)
        {
            if (b != null)
            {
                buildingsToSearch.Add(b.transform.position);
            }
        }
    }

    IEnumerator SearchBuilding_Sequence()
    {

        if (buildingsToSearch.Count > 0)
        {    

            print("เดินไปตึกเป้าหมาย");
            agent.SetDestination(buildingsToSearch[currentBuildingIndex]);

            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

            // เมื่อถึงแล้ว อาจจะรอสักพัก (Simulate การค้นหา)
            //isInsideBuilding = true;
            yield return StartCoroutine(BuildingSearchRoutine());
             //buildingsToSearch.RemoveAt(0);

        }

        else
        {
            agent.areaMask = NavMesh.AllAreas; //ปรับให้aiเดินได้ทุกพื้นที่
            scan_Building();

            // **เพิ่มบรรทัดนี้ลงไปเพื่อป้องกันเกมค้าง** // ถ้าไม่เจอตึกเลย ให้รอ 1 เฟรม แล้วสุ่มงานใหม่ 
            yield return new WaitForSeconds(1.5f);
            //random_Task();
        }
    }

    void EnterHouse()
    {
        // เวลาใช้ก็แค่สั่งบรรทัดเดียว สั้นและอ่านง่ายมาก
        agent.areaMask = insideHouseBit;
    }

   
    IEnumerator BuildingSearchRoutine()
    {
        EnterHouse(); // ล็อค AreaMask
        currentSearchpoint = 0;

        while (currentSearchpoint < MaxSearchPoint)
        {
            // สุ่มจุดใหม่โดยใช้ฟังก์ชันแยกที่นายทำไว้
            Vector3 nextPoint = GetRandomSearchPoint(buildingsToSearch[0], Search_inside_radius);
            agent.SetDestination(nextPoint);

            Debug.Log($"กำลังไปจุดสุ่มที่ {currentSearchpoint + 1}");

            // **สำคัญ**: รอจนกว่า Agent จะคำนวณทางและเดินไปถึงจุดสุ่มนั้นจริงๆ
            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

            yield return new WaitForSeconds(1.5f); // หยุดตรวจดูความเรียบร้อย
            Debug.Log("เดินไปที่จุดตรวจถัดไป");
            currentSearchpoint++;
        }

        // ตรวจครบทุกจุดในบ้านหลังนี้แล้ว ค่อยลบออกจาก List
        Debug.Log("ตรวจทั่วบ้านแล้ว ไปหลังต่อไป...");
        buildingsToSearch.RemoveAt(0);
        agent.areaMask = NavMesh.AllAreas; // ปลดล็อคพื้นที่
    }

    IEnumerator WaitAndMoveToNextBuilding()
    {
     // รอให้ AI เดินไปถึงจุดสุ่มในบ้านก่อน
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

        yield return new WaitForSeconds(2f); // หยุดส่ายหัวดูความเรียบร้อย 2 วิ

        //buildingsToSearch.RemoveAt(0); // ลบบ้านที่ตรวจเสร็จแล้วออก
             // กลับไปสถานะเดินหาบ้านหลังถัดไป
    }

    
    IEnumerator SurroundPlayerRoutine()
    {

        agent.areaMask = NavMesh.AllAreas; // เปิดพื้นที่ให้เดินได้เต็มที่
        // === เฟสที่ 1: ตรวจสอบรอบนอกก่อน (ตัดทางหนี) ===
        Debug.Log("AI เริ่มโอบล้อมรอบนอก...");
        for (int i = 0; i < 2; i++) // สุ่มตรวจรอบนอก 2 จุด
        {
            // สุ่มจุดที่ค่อนข้างไกล (รัศมี 15 ถึง 20 เมตร)
            Vector3 outerPoint = GetRing_RandomPoint(susPoint, 15f, 20f);
            agent.SetDestination(outerPoint);

            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
            yield return new WaitForSeconds(1.5f); // ยืนกวาดสายตาดู
        }

        // === เฟสที่ 2: กระชับพื้นที่เข้าสู่จุดศูนย์กลาง ===
        Debug.Log("AI บีบวงล้อมเข้าหาจุดเกิดเหตุ...");
        for (int i = 0; i < 2; i++) // บีบวงล้อมเข้ามาตรวจจุดที่แคบลง 2 จุด
        {
            // สุ่มจุดที่ใกล้จุดเกิดเหตุมากขึ้น (รัศมี 3 ถึง 8 เมตร)
            Vector3 innerPoint = GetRing_RandomPoint(susPoint, 3f, 8f);
            agent.SetDestination(innerPoint);

            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
            yield return new WaitForSeconds(1.5f);
        }

        // === เฟสที่ 3: เดินค้นหาซอกหลืบตรงจุดเกิดเหตุเป๊ะๆ ===
        Debug.Log("AI ถึงจุดเกิดเหตุหลัก... กำลังตรวจค้นอย่างละเอียด");
        agent.SetDestination(susPoint); // เดินไปเหยียบตรงจุดเกิดเหตุตรงๆ ก่อน 1 ครั้ง
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
        yield return new WaitForSeconds(2.5f); // ยืนส่องไฟฉายค้นหาอย่างละเอียด

        // จบการค้นหา ย้ายไปทำ Task อื่นต่อ
        //isSurround = false;
        //isScanBuilding = false; // เคลียร์สถานะเพื่อให้สแกนหาตึกต่อได้
        //random_Task(); // สุ่มงานใหม่
    }

   

    IEnumerator KeepPosition_Sequence()
    {
        while (enemy_script.currentState == enemy_stage.EnemyState.alertSearching)
        {
            yield return StartCoroutine(Relocate());

            SettingKeepPosition();

            yield return StartCoroutine(KeepPositionRoutine());
        }
    }

    void SettingKeepPosition() 
    {

            CurrentPosition = transform.position;
            //isKeepPosition = true;
            agent.areaMask = NavMesh.AllAreas; // เปิดให้เดินได้ทั่วไป ไม่ต้องโดนขังในบ้านแล้ว
            //StartCoroutine(KeepPositionRoutine());
            
    }

    IEnumerator Relocate()
    {
        Vector3 Relocate_point = GetRandomSearchPoint(transform.position, 50f);

        agent.SetDestination(Relocate_point);
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

        yield return new WaitForSeconds(2f);
        
    }

    IEnumerator KeepPositionRoutine()
    {
        // ให้มันเดินวนตรวจแถวนั้นไปเรื่อย ๆ (เช่น สุ่มตรวจสัก 4 จุดเหมือนระบบในบ้านของนาย)
        int checkCount = 0;
        int maxChecks = 6;

        while (checkCount < maxChecks && enemy_script.currentState == enemy_stage.EnemyState.alertSearching)
        {
            // สั่งสุ่มจุดโดยใช้พิกัดล่าสุดล็อกไว้เป็นจุดศูนย์กลาง
            Vector3 nextSearchPoint = GetRandomSearchPoint(CurrentPosition, searchRadiusAroundPoint);
            agent.SetDestination(nextSearchPoint);

            Debug.Log($"กำลังตรวจสอบรอบพิกัดล่าสุด จุดย่อยที่ {checkCount + 1}");

            // รอจนเดินถึงจุดสุ่มย่อยนั้น ๆ
            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

            // ยืนส่องไฟฉาย/มองหา 2 วินาที
            yield return new WaitForSeconds(2f);
            checkCount++;
        }

    }

    void hint_playerPos() //ใบ้ตำแหน่งผู้เล่น แบบกว้างๆ
    {

    }

    void random_Task() //สุ่มว่าจะได้ทำอะไรระหว่าง keepPosition กับ search_Building
    {
        // 1. สุ่มตัวเลขระหว่าง 0 ถึง 100
        float chance = Random.Range(0f, 100f);

        // 2. ใช้เงื่อนไขแบ่งเปอร์เซ็นต์ (เช่น ค้นบ้าน 70% / คุมพื้นที่ 30%)
        if (chance <= 60f)
        {
            // โอกาส 70%: ไปค้นตามซอกตึกหรือในบ้าน
            Debug.Log("AI ตัดสินใจเข้าตรวจค้นอาคาร");
            currentSearch = SearchType.serach_building;
        }
        else
        {
            // โอกาส 30%: ยืนคุมเชิง หรือเดินลาดตระเวนรอบๆ โซน
            Debug.Log("AI ตัดสินใจคุมพื้นที่");
            currentSearch = SearchType.keepposition;
        }
    } 
    
     Vector3 GetRing_RandomPoint(Vector3 center, float minRad, float maxRad)
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(minRad, maxRad);

        Vector3 targetPos = center + new Vector3(randomDir.x * randomDist, 0, randomDir.y * randomDist);
        targetPos.y = center.y;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }
    
    Vector3 GetRandomSearchPoint(Vector3 center, float radius)
    {
        // 1. สุ่มพิกัดรอบๆ จุดศูนย์กลาง
        Vector3 randomPos = center + (Random.insideUnitSphere * radius);
        randomPos.y = center.y; // คุมระดับความสูงให้เท่ากับตัวบ้าน

        NavMeshHit hit;
        // 2. ตรวจสอบว่าพิกัดนั้นอยู่บน NavMesh ที่ Agent เดินได้หรือไม่
        // ใช้ areaMask ของ Agent ในขณะนั้นมาเป็นตัวกรอง (เพื่อให้ติดกฎของ Modifier Volume)
        if (NavMesh.SamplePosition(randomPos, out hit, 5f, agent.areaMask))
        {
            return hit.position; // ส่งพิกัดที่เดินได้จริงกลับไป
        }

        return center; // ถ้าหาไม่ได้จริงๆ ให้ส่งจุดศูนย์กลางบ้านกลับไปแทนเพื่อความปลอดภัย
    }
}
