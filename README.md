# Graduation

<개요>
장르: 3인칭 액션 RPG (3인칭 숄더뷰)
플랫폼: PC(Windows)
개발환경: Unity3D / C#

진행방식: 던전을 탐험하며 조우하는 몬스터와의 전투를 중심으로 전개. 최종적으로 마지막 스테이지에 있는 보스 몬스터를 처치하면 승리.
그래픽 리소스: 메인 – POLYGON – Dungeons Pack (Unity Asset Store)
그 외 유니티 Asset Store 무료 리소스 사용
스토리: 사악한 몬스터들에게 납치된 공주를 구하러 가는 용사(플레이어)의 이야기


<구현중점 사항>
가비지 컬렉션(Garbage Collection)발생을 염두에 두고 Object Pooling을 통해 투사체, 이펙트, 파티클을 재사용. 동적 메모리 할당으로 인한 메모리 누수와 GC발생을 방지하고자 함
Managers, Player 등 중복해서 존재해서는 안되는 인스턴스에는 싱글톤(Singleton) 패턴을 적용해 관리하고자 함
상속을 통한 중복 코드 제거, 유닛 클래스들의 그룹화를 위해 추상클래스와, 인터페이스를 활용해 클래스를 설계하고자 함
Update method호출 최소화를 위해 피격 처리와 같은 부분은 피격당한 유닛에게 DamageMessage를 보내 처리하는 등, event 기반으로 처리하고자 함
