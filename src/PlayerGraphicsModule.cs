using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

//玩家图像相关功能的实际实现
public class PlayerGraphicsModule
{
    public PlayerGraphicsModule(PlayerGraphics playerGraphics)
    {
        self = playerGraphics;
    }

    /*-----------------------------------------------------字段-----------------------------------------------------*/
    //玩家图像
    public PlayerGraphics self;
    //尾巴图像元素的集合（？
    public FAtlas tailAtlas;
    //尾巴的网格
    public TriangleMesh tailmesh;
    //模拟尾巴旋转的UV偏移距离
    public float movelength;
    /*-----------------------------------------------------方法-----------------------------------------------------*/
    //绘制新的尾巴
    public void InitiatSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        //从文件夹中读取尾巴的图像素材
        tailAtlas = Futile.atlasManager.LoadAtlas("atlases/phosphorscales");

        //建立一系列的三角形
        TriangleMesh.Triangle[] tailtris = new TriangleMesh.Triangle[]
        {
            new TriangleMesh.Triangle(0,1,2),
            new TriangleMesh.Triangle(1,2,3),
            new TriangleMesh.Triangle(2,3,4),
            new TriangleMesh.Triangle(3,4,5),
            new TriangleMesh.Triangle(4,5,6),
            new TriangleMesh.Triangle(5,6,7),
            new TriangleMesh.Triangle(6,7,8),
            new TriangleMesh.Triangle(7,8,9),
            new TriangleMesh.Triangle(8,9,10),
            new TriangleMesh.Triangle(9,10,11),
            new TriangleMesh.Triangle(10,11,12),
            new TriangleMesh.Triangle(11,12,13),
            new TriangleMesh.Triangle(12,13,14),
        };

        //从三角形和图像素材建立尾巴的三角网格
        tailmesh = new TriangleMesh("phosphorscales", tailtris, true, true);

        //尾巴形状的粗略调整（我调的形状好丑啊受不了了但是真的调不来（留言：咱觉得挺不错的qwq
        self.tail[0] = new TailSegment(self, 8f, 8f, null, 0.85f, 1f, 1f, true);
        self.tail[1] = new TailSegment(self, 8f, 10f, self.tail[0], 0.85f, 1f, 0.5f, true);
        self.tail[2] = new TailSegment(self, 6f, 10f, self.tail[1], 0.85f, 1f, 0.5f, true);
        self.tail[3] = new TailSegment(self, 6f, 10f, self.tail[2], 0.85f, 1f, 0.5f, true);

        //将原本的尾巴替换为新绘制的尾巴
        sLeaser.sprites[2] = tailmesh;
        sLeaser.sprites[2].MoveInFrontOfOtherNode(sLeaser.sprites[4]);

        //成啦----
        self.AddToContainer(sLeaser, rCam, null);
    }

    //替换角色贴图、指定UV顶点位置
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        //替换角色身体部件的贴图
        //i=0,1,2,3,4,5,6,7,8,9 但是等于2（尾巴）时跳过，也就是说替换除了尾巴之外的所有贴图
        for (int i = 0; i < 9; i++)
        {
            if (i == 2) continue;
            //找到名字开头不是Traveler的element,替换为有这个前缀的版本
            if (!sLeaser.sprites[i].element.name.StartsWith("Traveler"))
            {
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("Traveler" + sLeaser.sprites[i].element.name);
            }

        }

        //修正新尾巴的UV顶点位置，同时加上控制UV顶点移动的变量movelength
        //实际上只截取到了半张尾部贴图，为滚UV留出空间（不知是否有这个必要？
        //此部分参考了哈维老师之前给出的写法
        float l = tailAtlas._elementsByName["phosphorscales"].uvTopLeft.y - tailAtlas._elementsByName["phosphorscales"].uvBottomLeft.y;
        int step1 = (tailmesh.UVvertices.Length - 2) / 2;
        int step2 = step1 + 1;
        float x = 0f;

        //UV在垂直方向滚动
        for (int i = 0; i < tailmesh.UVvertices.Length; i++)
        {
            bool isUpper = i % 2 == 0;
            float f1;
            float f2;
            Vector2 uv = Vector2.zero;
            uv.x = Mathf.Lerp(tailAtlas._elementsByName["phosphorscales"].uvBottomLeft.x, tailAtlas._elementsByName["phosphorscales"].uvTopRight.x, x);
            uv.y = Mathf.Lerp(tailAtlas._elementsByName["phosphorscales"].uvBottomLeft.y, tailAtlas._elementsByName["phosphorscales"].uvBottomLeft.y + 0.5f * l, (isUpper ? 0f : 1f)) + 0.5f * movelength;
            if (isUpper)
            {
                f1 = (float)i;
                f2 = (float)2 * step2;
            }
            else
            {
                f1 = (float)(i - 1);
                f2 = (float)2 * step1;
            }
            x = f1 / f2;
            tailmesh.UVvertices[i] = uv;
        }
    }

    //非常之简陋的滚UV效果，通过玩家最靠近臀部的一截尾巴与躯干的夹角来决定UV顶点移动的距离，实现简单的旋转效果
    public void RollUpdate()
    {
        float h = tailAtlas._elementsByName["phosphorscales"].uvTopRight.y - tailAtlas._elementsByName["phosphorscales"].uvBottomLeft.y;
        //玩家躯干的方向
        Vector2 dir = (self.owner.bodyChunks[1].pos - self.owner.bodyChunks[0].pos).normalized;
        //玩家最靠近臀部的一截尾巴的方向
        Vector2 dir2 = (self.tail[1].pos - self.tail[0].pos).normalized;
        //dir和dir2之间的夹角（只取0~90°范围
        float num = Mathf.Clamp(Vector2.Angle(dir, dir2), 0, 90);
        //UV顶点移动距离
        movelength = num * (h / 90);
    }
}
