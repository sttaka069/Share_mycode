// ========================================
// 1. クラスの先頭に追加する変数
// ========================================

/// <summary>
/// 敵キャラクターのプレハブ
/// </summary>
public GameObject enemyPrefab;

/// <summary>
/// 宝箱のプレハブ
/// </summary>
public GameObject treasurePrefab;

/// <summary>
/// マップ上に配置する敵の数
/// </summary>
public int enemyCount = 10;

/// <summary>
/// マップ上に配置する宝箱の数
/// </summary>
public int treasureCount = 5;

/// <summary>
/// 歩行可能な床タイルの座標リスト
/// </summary>
private List<Vector2Int> floorPositions = new List<Vector2Int>();


// ========================================
// 2. Generate()メソッドの変更
// ========================================

void Generate()
{
    tilemap.ClearAllTiles();
    rooms.Clear();
    floorPositions.Clear(); // ← この行を追加（床座標リストの初期化）
    
    // まず全部を壁にする
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
        }
    }
  
    // 部屋生成
    for (int i = 0; i < roomCount; i++)
    {
        int w = Random.Range(roomMinSize, roomMaxSize);
        int h = Random.Range(roomMinSize, roomMaxSize);
        int x = Random.Range(1, width - w - 1);
        int y = Random.Range(1, height - h - 1);
        Rect room = new Rect(x, y, w, h);
        rooms.Add(room);
        
        // 部屋の床を作る
        CreateRoom(room);
        
        // 前の部屋と繋げる
        if (i > 0)
        {
            Vector2 prevCenter = rooms[i - 1].center;
            Vector2 newCenter = room.center;
            CreateCorridor(prevCenter, newCenter);
        }
    }
    
    // 敵と宝箱を配置（← この行を追加）
    SpawnObjects();
}


// ========================================
// 3. CreateRoom()メソッドの変更
// ========================================

/// <summary>
/// 部屋を生成し、床タイルを配置する
/// </summary>
/// <param name="room">生成する部屋の矩形情報</param>
void CreateRoom(Rect room)
{
    for (int x = (int)room.xMin; x < room.xMax; x++)
    {
        for (int y = (int)room.yMin; y < room.yMax; y++)
        {
            // 床タイルを配置
            tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            
            // 床の座標を記録（オブジェクト配置用）← この行を追加
            floorPositions.Add(new Vector2Int(x, y));
        }
    }
}


// ========================================
// 4. CreateCorridor()メソッドの変更
// ========================================

/// <summary>
/// 2つの部屋をL字型の通路で接続する
/// </summary>
/// <param name="a">開始地点</param>
/// <param name="b">終了地点</param>
void CreateCorridor(Vector2 a, Vector2 b)
{
    // 横方向の通路を作成
    for (int x = (int)a.x; x != (int)b.x; x += a.x < b.x ? 1 : -1)
    {
        tilemap.SetTile(new Vector3Int(x, (int)a.y, 0), floorTile);
        
        // 重複チェックして床座標を記録（← この2行を追加）
        if (!floorPositions.Contains(new Vector2Int(x, (int)a.y)))
            floorPositions.Add(new Vector2Int(x, (int)a.y));
    }
    
    // 縦方向の通路を作成
    for (int y = (int)a.y; y != (int)b.y; y += a.y < b.y ? 1 : -1)
    {
        tilemap.SetTile(new Vector3Int((int)b.x, y, 0), floorTile);
        
        // 重複チェックして床座標を記録（← この2行を追加）
        if (!floorPositions.Contains(new Vector2Int((int)b.x, y)))
            floorPositions.Add(new Vector2Int((int)b.x, y));
    }
}


// ========================================
// 5. クラスの最後に追加する新しいメソッド
// ========================================

/// <summary>
/// マップ上に敵と宝箱をランダムに配置する
/// </summary>
private void SpawnObjects()
{
    // 配置可能な座標リストを作成（元のリストのコピー）
    List<Vector2Int> availablePositions = new List<Vector2Int>(floorPositions);
    
    // プレイヤーの初期位置（最初の部屋の中央）を配置候補から除外
    if (rooms.Count > 0)
    {
        Vector2Int startPos = new Vector2Int((int)rooms[0].center.x, (int)rooms[0].center.y);
        availablePositions.Remove(startPos);
    }

    // 敵を配置
    for (int i = 0; i < enemyCount && availablePositions.Count > 0; i++)
    {
        // ランダムな位置を選択
        int index = Random.Range(0, availablePositions.Count);
        Vector2Int pos = availablePositions[index];
        
        // 選択した位置を候補から削除（重複配置を防ぐ）
        availablePositions.RemoveAt(index);
        
        // 敵プレハブが設定されていれば生成
        if (enemyPrefab != null)
        {
            // タイル座標をワールド座標に変換
            Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(pos.x, pos.y, 0));
            
            // タイルの中央に配置するため、オフセットを追加
            worldPos += tilemap.cellSize / 2;
            
            // 敵を生成（親オブジェクトをMapGeneratorに設定）
            Instantiate(enemyPrefab, worldPos, Quaternion.identity, transform);
        }
    }

    // 宝箱を配置
    for (int i = 0; i < treasureCount && availablePositions.Count > 0; i++)
    {
        // ランダムな位置を選択
        int index = Random.Range(0, availablePositions.Count);
        Vector2Int pos = availablePositions[index];
        
        // 選択した位置を候補から削除（重複配置を防ぐ）
        availablePositions.RemoveAt(index);
        
        // 宝箱プレハブが設定されていれば生成
        if (treasurePrefab != null)
        {
            // タイル座標をワールド座標に変換
            Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(pos.x, pos.y, 0));
            
            // タイルの中央に配置するため、オフセットを追加
            worldPos += tilemap.cellSize / 2;
            
            // 宝箱を生成（親オブジェクトをMapGeneratorに設定）
            Instantiate(treasurePrefab, worldPos, Quaternion.identity, transform);
        }
    }
}