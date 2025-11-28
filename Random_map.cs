using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// ランダムなダンジョンマップを生成するクラス
/// </summary>
public class MapGenerator : MonoBehaviour
{
    // ========================================
    // マップサイズ設定
    // ========================================
    
    /// <summary>
    /// マップの幅（タイル数）
    /// </summary>
    public int width = 100;
    
    /// <summary>
    /// マップの高さ（タイル数）
    /// </summary>
    public int height = 100;
    
    
    // ========================================
    // 部屋生成設定
    // ========================================
    
    /// <summary>
    /// 生成する部屋の数
    /// </summary>
    public int roomCount = 5;
    
    /// <summary>
    /// 部屋の最小サイズ
    /// </summary>
    public int roomMinSize = 6;
    
    /// <summary>
    /// 部屋の最大サイズ
    /// </summary>
    public int roomMaxSize = 15;
    
    
    // ========================================
    // タイル設定
    // ========================================
    
    /// <summary>
    /// 壁に使用するタイル
    /// </summary>
    public TileBase wallTile;
    
    /// <summary>
    /// 床に使用するタイル
    /// </summary>
    public TileBase floorTile;
    
    /// <summary>
    /// タイルを配置するTilemap
    /// </summary>
    public Tilemap tilemap;
    
    
    // ========================================
    // 内部データ
    // ========================================
    
    /// <summary>
    /// 生成された部屋のリスト
    /// </summary>
    private List<Rect> rooms = new List<Rect>();
    
    
    // ========================================
    // 初期化
    // ========================================
    
    /// <summary>
    /// 開始時にマップを生成
    /// </summary>
    void Start()
    {
        Generate();
    }
    
    
    // ========================================
    // マップ生成メイン処理
    // ========================================
    
    /// <summary>
    /// ランダムなダンジョンマップを生成する
    /// </summary>
    void Generate()
    {
        // 既存のタイルと部屋データをクリア
        tilemap.ClearAllTiles();
        rooms.Clear();
        
        // ステップ1: マップ全体を壁で埋める
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
            }
        }
        
        // ステップ2: 部屋を生成して通路で繋ぐ
        for (int i = 0; i < roomCount; i++)
        {
            // ランダムなサイズの部屋を生成
            int w = Random.Range(roomMinSize, roomMaxSize);
            int h = Random.Range(roomMinSize, roomMaxSize);
            
            // ランダムな位置に配置（マップの端を避ける）
            int x = Random.Range(1, width - w - 1);
            int y = Random.Range(1, height - h - 1);
            
            // 部屋情報を作成してリストに追加
            Rect room = new Rect(x, y, w, h);
            rooms.Add(room);
            
            // 部屋の床を作成
            CreateRoom(room);
            
            // 2つ目以降の部屋は、前の部屋と通路で繋ぐ
            if (i > 0)
            {
                Vector2 prevCenter = rooms[i - 1].center;
                Vector2 newCenter = room.center;
                CreateCorridor(prevCenter, newCenter);
            }
        }
    }
    
    
    // ========================================
    // 部屋生成
    // ========================================
    
    /// <summary>
    /// 指定された矩形範囲に部屋（床タイル）を作成する
    /// </summary>
    /// <param name="room">生成する部屋の矩形情報</param>
    void CreateRoom(Rect room)
    {
        // 部屋の範囲内すべてに床タイルを配置
        for (int x = (int)room.xMin; x < room.xMax; x++)
        {
            for (int y = (int)room.yMin; y < room.yMax; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }
    
    
    // ========================================
    // 通路生成
    // ========================================
    
    /// <summary>
    /// 2つの地点をL字型の通路で接続する
    /// </summary>
    /// <param name="a">開始地点の座標</param>
    /// <param name="b">終了地点の座標</param>
    void CreateCorridor(Vector2 a, Vector2 b)
    {
        // 横方向の通路を作成（a地点からb地点のX座標まで）
        for (int x = (int)a.x; x != (int)b.x; x += a.x < b.x ? 1 : -1)
        {
            tilemap.SetTile(new Vector3Int(x, (int)a.y, 0), floorTile);
        }
        
        // 縦方向の通路を作成（a地点のY座標からb地点まで）
        for (int y = (int)a.y; y != (int)b.y; y += a.y < b.y ? 1 : -1)
        {
            tilemap.SetTile(new Vector3Int((int)b.x, y, 0), floorTile);
        }
    }
}