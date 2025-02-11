using System.Text.Json;

using Samples.Json;

namespace Ramstack.Parsing.Scenarios;

[TestFixture]
public class JsonTests
{
    [Test]
    public void JsonParseTest()
    {
        var s1 = JsonSerializer.Serialize(JsonParser.Parser.Parse(Json).Value);
        var s2 = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(Json));
        Assert.That(s1, Is.EqualTo(s2));
    }

    private const string Json =
        """
        [
          {
            "_id": "5420ea1c9d34f914b83bc28a",
            "index": 0.1,
            "guid": "88ccc7c0-e2dd-4523-8066-84c58f0dfd5d",
            "isActive": false,
            "balance": "$1,553.84",
            "picture": "https://placehold.it/32x32",
            "age": 26.1,
            "eyeColor": "green",
            "name": "Goodwin Cherry",
            "gender": "male",
            "company": "DIGIPRINT",
            "email": "goodwincherry@digiprint.com",
            "phone": "+1 (809) 545-2897",
            "address": "894 Montauk Avenue, Wyoming, Virgin Islands, 7416",
            "about": "Id aute esse amet tempor excepteur in ex aute velit irure. Proident mollit minim fugiat ex non anim Lorem mollit est ullamco consectetur dolor officia adipisicing. Voluptate ut et non quis nisi. Quis occaecat commodo excepteur et deserunt fugiat sit occaecat. Deserunt incididunt laboris nisi sint irure nostrud sint amet et id. Ut pariatur in eiusmod sit excepteur.\r\n",
            "registered": "2014-06-14T20:34:23-12:00",
            "latitude": -89.19685,
            "longitude": 32.263679,
            "tags": [
              "aliqua",
              "esse",
              "non",
              "officia",
              "enim",
              "nostrud",
              "consectetur"
            ],
            "friends": [
              {
                "id": 1.1,
                "name": "Saundra Lucas"
              },
              {
                "id": 1.2,
                "name": "Rachelle Hancock"
              },
              {
                "id": 1.3,
                "name": "Sharp Wilkerson"
              }
            ],
            "greeting": "Hello, Goodwin Cherry! You have 9 unread messages.",
            "favoriteFruit": "banana"
          },
          {
            "_id": "5420ea1c2ce68275fc87b664",
            "index": 1.1,
            "guid": "63e815df-7102-42ca-96ae-adf7915dbd1f",
            "isActive": false,
            "balance": "$2,184.56",
            "picture": "https://placehold.it/32x32",
            "age": 23.1,
            "eyeColor": "green",
            "name": "Perez Zamora",
            "gender": "male",
            "company": "SHADEASE",
            "email": "perezzamora@shadease.com",
            "phone": "+1 (843) 562-2745",
            "address": "902 Noel Avenue, Goldfield, Massachusetts, 5555",
            "about": "Labore sunt qui consequat anim veniam voluptate aliqua. Tempor ullamco est culpa anim incididunt ipsum culpa veniam et ut ad excepteur commodo Lorem. Laboris duis do amet culpa sunt in velit incididunt nisi ex dolor. Lorem do sint veniam sint pariatur qui reprehenderit. Nisi in cupidatat eu irure esse et ipsum Lorem.\r\n",
            "registered": "2014-06-11T11:33:57-12:00",
            "latitude": 52.963756,
            "longitude": -97.251673,
            "tags": [
              "commodo",
              "do",
              "ullamco",
              "duis",
              "aliqua",
              "adipisicing",
              "aliquip"
            ],
            "friends": [
              {
                "id": 1.1,
                "name": "Strickland Mack"
              },
              {
                "id": 1.2,
                "name": "Allison Oneill"
              },
              {
                "id": 1.3,
                "name": "Hampton Lawson"
              }
            ],
            "greeting": "Hello, Perez Zamora! You have 4 unread messages.",
            "favoriteFruit": "banana"
          },
          {
            "_id": "5420ea1c101c26a29fc0476d",
            "index": 2.1,
            "guid": "362d0d30-2aa3-4a14-b292-908288c05e1f",
            "isActive": false,
            "balance": "$2,429.42",
            "picture": "https://placehold.it/32x32",
            "age": 32.1,
            "eyeColor": "blue",
            "name": "Jenifer Thomas",
            "gender": "female",
            "company": "OPTIQUE",
            "email": "jeniferthomas@optique.com",
            "phone": "+1 (882) 462-3944",
            "address": "820 Hausman Street, Yardville, Utah, 3474",
            "about": "Eiusmod voluptate voluptate culpa est adipisicing officia nisi id irure voluptate irure do dolore. Laboris ipsum irure qui ut in. In ipsum nulla adipisicing eu reprehenderit deserunt excepteur culpa ea in nisi ut.\r\n",
            "registered": "2014-09-09T05:57:09-12:00",
            "latitude": 18.683324,
            "longitude": -63.517571,
            "tags": [
              "enim",
              "cupidatat",
              "velit",
              "officia",
              "irure",
              "mollit",
              "culpa"
            ],
            "friends": [
              {
                "id": 1.1,
                "name": "Ilene Stein"
              },
              {
                "id": 1.2,
                "name": "Albert Jacobs"
              },
              {
                "id": 1.3,
                "name": "Flores Malone"
              }
            ],
            "greeting": "Hello, Jenifer Thomas! You have 8 unread messages.",
            "favoriteFruit": "strawberry"
          }
        ]
        """;
}
