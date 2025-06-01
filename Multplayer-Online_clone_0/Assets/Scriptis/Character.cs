using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character")]
public class Character : ScriptableObject
{
    /*Creditos: 
        Dapper Dino:https://www.youtube.com/@DapperDinoCodingTutorials
    */
    [SerializeField] private string characterName = default;//Nome do personagem
    [SerializeField] private GameObject characterPreviewPrefab = default;//Prefab de apresentação do personagem
    [SerializeField] private GameObject gameplayCharacterPrefab = default;//Prefab do jogavel do player
    //Informações do player para uso nos outros scripts
    public string CharacterName => characterName;
    public GameObject CharacterPreviewPrefab => characterPreviewPrefab;
    public GameObject GameplayCharacterPrefab => gameplayCharacterPrefab;
}
