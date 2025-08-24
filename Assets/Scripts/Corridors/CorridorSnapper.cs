using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ZhouSoftware
{
    public class CorridorSnapper : MonoBehaviour
    {
        [SerializeField] Transform lastSegment;         // 上一段的根
        [SerializeField] Transform lastSocketOut;       // 上一段的 Socket_Out
        [SerializeField] GameObject segmentPrefab;      // 走廊预制体
        [SerializeField] string socketInName = "Socket_In";
        [SerializeField] string socketOutName = "Socket_Out";

            /// <param name="flip">是否做“回廊翻转”（绕Y轴180°）</param>
            public Transform AddNext(bool flip)
            {
                // 1) 生成下一段（临时姿态无所谓）
                var next = Instantiate(segmentPrefab).transform;

                // 2) 取出插口
                var nextSocketIn = next.Find(socketInName);
                var nextSocketOut = next.Find(socketOutName);
                if (!nextSocketIn || !lastSocketOut)
                {
                    Debug.LogError("Socket 未找到：请确认命名与层级正确。");
                    Destroy(next.gameObject);
                    return null;
                }

                // 3) 计算目标旋转：先把 next 的入口朝向对齐到 last 的出口，
                //    如需回廊翻转，再额外乘一个 Y 轴 180°
                Quaternion extra = flip ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
                Quaternion targetRot = lastSocketOut.rotation * extra * Quaternion.Inverse(nextSocketIn.rotation);

                // 4) 先设旋转，再做位置对齐（先旋转是为了保证入口的朝向正确）
                next.rotation = targetRot;

                // 让 next 的入口点与 last 的出口点重合
                Vector3 delta = lastSocketOut.position - nextSocketIn.position;
                next.position += delta;

                // 5) 更新“上一段”的引用，方便连续生成
                lastSegment = next;
                lastSocketOut = nextSocketOut;
                return next;
        }
    }
}