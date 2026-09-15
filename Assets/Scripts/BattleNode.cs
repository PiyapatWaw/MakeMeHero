using System.Collections.Generic;
using UnityEngine;

public class BattleNode : MonoBehaviour
{
    [SerializeField] private int column;
    [SerializeField] private int row;

    [SerializeField] private List<BattleNode> connections = new();

    public int Column => column;
    public int Row => row;

    public IReadOnlyList<BattleNode> Connections => connections;

    public void Initialize(int column, int row)
    {
        this.column = column;
        this.row = row;

        connections.Clear();
    }

    public void AddConnection(BattleNode node)
    {
        if (node == null || node == this)
            return;

        if (!connections.Contains(node))
            connections.Add(node);
    }

    public void ClearConnections()
    {
        connections.Clear();
    }
}