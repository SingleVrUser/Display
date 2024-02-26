using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actor_Names",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actor_Names", x => new { x.id, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "Actor_Video",
                columns: table => new
                {
                    actor_id = table.Column<int>(type: "INTEGER", nullable: false),
                    video_name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actor_Video", x => new { x.actor_id, x.video_name });
                });

            migrationBuilder.CreateTable(
                name: "ActorInfo",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    is_woman = table.Column<int>(type: "INTEGER", nullable: true),
                    birthday = table.Column<string>(type: "TEXT", nullable: true),
                    bwh = table.Column<string>(type: "TEXT", nullable: true),
                    height = table.Column<int>(type: "INTEGER", nullable: true),
                    works_count = table.Column<int>(type: "INTEGER", nullable: true),
                    work_time = table.Column<string>(type: "TEXT", nullable: true),
                    prifile_path = table.Column<string>(type: "TEXT", nullable: true),
                    blog_url = table.Column<string>(type: "TEXT", nullable: true),
                    is_like = table.Column<int>(type: "INTEGER", nullable: true),
                    addtime = table.Column<int>(type: "INTEGER", nullable: true),
                    info_url = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActorInfo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bwh",
                columns: table => new
                {
                    bwh = table.Column<string>(type: "TEXT", nullable: false),
                    bust = table.Column<int>(type: "INTEGER", nullable: true),
                    waist = table.Column<int>(type: "INTEGER", nullable: true),
                    hips = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bwh", x => x.bwh);
                });

            migrationBuilder.CreateTable(
                name: "DownHistory",
                columns: table => new
                {
                    file_pickcode = table.Column<string>(type: "TEXT", nullable: false),
                    ua = table.Column<string>(type: "TEXT", nullable: false),
                    file_name = table.Column<string>(type: "TEXT", nullable: true),
                    true_url = table.Column<string>(type: "TEXT", nullable: true),
                    add_time = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownHistory", x => new { x.file_pickcode, x.ua });
                });

            migrationBuilder.CreateTable(
                name: "FailList_islike_looklater",
                columns: table => new
                {
                    pc = table.Column<string>(type: "TEXT", nullable: false),
                    is_like = table.Column<int>(type: "INTEGER", nullable: true),
                    score = table.Column<int>(type: "INTEGER", nullable: true),
                    look_later = table.Column<int>(type: "INTEGER", nullable: true),
                    image_path = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailList_islike_looklater", x => x.pc);
                });

            migrationBuilder.CreateTable(
                name: "FilesInfo",
                columns: table => new
                {
                    pc = table.Column<string>(type: "TEXT", nullable: false),
                    fid = table.Column<string>(type: "TEXT", nullable: true),
                    uid = table.Column<int>(type: "INTEGER", nullable: true),
                    aid = table.Column<int>(type: "INTEGER", nullable: true),
                    cid = table.Column<string>(type: "TEXT", nullable: true),
                    n = table.Column<string>(type: "TEXT", nullable: true),
                    s = table.Column<int>(type: "INTEGER", nullable: true),
                    sta = table.Column<int>(type: "INTEGER", nullable: true),
                    pt = table.Column<string>(type: "TEXT", nullable: true),
                    pid = table.Column<string>(type: "TEXT", nullable: true),
                    p = table.Column<int>(type: "INTEGER", nullable: true),
                    m = table.Column<int>(type: "INTEGER", nullable: true),
                    t = table.Column<string>(type: "TEXT", nullable: true),
                    te = table.Column<int>(type: "INTEGER", nullable: true),
                    tp = table.Column<int>(type: "INTEGER", nullable: true),
                    d = table.Column<int>(type: "INTEGER", nullable: true),
                    c = table.Column<int>(type: "INTEGER", nullable: true),
                    sh = table.Column<int>(type: "INTEGER", nullable: true),
                    e = table.Column<string>(type: "TEXT", nullable: true),
                    ico = table.Column<string>(type: "TEXT", nullable: true),
                    sha = table.Column<string>(type: "TEXT", nullable: true),
                    fdes = table.Column<string>(type: "TEXT", nullable: true),
                    q = table.Column<int>(type: "INTEGER", nullable: true),
                    hdf = table.Column<int>(type: "INTEGER", nullable: true),
                    fvs = table.Column<int>(type: "INTEGER", nullable: true),
                    u = table.Column<string>(type: "TEXT", nullable: true),
                    iv = table.Column<int>(type: "INTEGER", nullable: true),
                    current_time = table.Column<int>(type: "INTEGER", nullable: true),
                    played_end = table.Column<int>(type: "INTEGER", nullable: true),
                    last_time = table.Column<string>(type: "TEXT", nullable: true),
                    vdi = table.Column<int>(type: "INTEGER", nullable: true),
                    play_long = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesInfo", x => x.pc);
                });

            migrationBuilder.CreateTable(
                name: "FileToInfo",
                columns: table => new
                {
                    file_pickcode = table.Column<string>(type: "TEXT", nullable: false),
                    truename = table.Column<string>(type: "TEXT", nullable: true),
                    issuccess = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileToInfo", x => x.file_pickcode);
                });

            migrationBuilder.CreateTable(
                name: "Is_Wm",
                columns: table => new
                {
                    truename = table.Column<string>(type: "TEXT", nullable: false),
                    is_wm = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Is_Wm", x => x.truename);
                });

            migrationBuilder.CreateTable(
                name: "ProducerInfo",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    is_wm = table.Column<int>(type: "is_wm", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerInfo", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "SearchHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    keyword = table.Column<string>(type: "TEXT NO", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "SpiderLog",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "INTEGER", nullable: false),
                    time = table.Column<string>(type: "TEXT", nullable: true),
                    done = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiderLog", x => x.task_id);
                });

            migrationBuilder.CreateTable(
                name: "SpiderTask",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    bus = table.Column<string>(type: "TEXT", nullable: true),
                    Jav321 = table.Column<string>(type: "TEXT", nullable: true),
                    Avmoo = table.Column<string>(type: "TEXT", nullable: true),
                    Avsox = table.Column<string>(type: "TEXT", nullable: true),
                    libre = table.Column<string>(type: "TEXT", nullable: true),
                    fc = table.Column<string>(type: "TEXT", nullable: true),
                    db = table.Column<string>(type: "TEXT", nullable: true),
                    done = table.Column<int>(type: "INTEGER", nullable: true),
                    tadk_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiderTask", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "VideoInfo",
                columns: table => new
                {
                    truename = table.Column<string>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: true),
                    releasetime = table.Column<string>(type: "TEXT", nullable: true),
                    lengthtime = table.Column<string>(type: "TEXT", nullable: true),
                    director = table.Column<string>(type: "TEXT", nullable: true),
                    producer = table.Column<string>(type: "TEXT", nullable: true),
                    publisher = table.Column<string>(type: "TEXT", nullable: true),
                    series = table.Column<string>(type: "TEXT", nullable: true),
                    category = table.Column<string>(type: "TEXT", nullable: true),
                    actor = table.Column<string>(type: "TEXT", nullable: true),
                    imageurl = table.Column<string>(type: "TEXT", nullable: true),
                    sampleImageList = table.Column<string>(type: "TEXT", nullable: true),
                    imagepath = table.Column<string>(type: "TEXT", nullable: true),
                    busurl = table.Column<string>(type: "TEXT", nullable: true),
                    look_later = table.Column<int>(type: "INTEGER", nullable: true),
                    score = table.Column<int>(type: "INTEGER", nullable: true),
                    is_like = table.Column<int>(type: "INTEGER", nullable: true),
                    addtime = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoInfo", x => x.truename);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchHistory_keyword",
                table: "SearchHistory",
                column: "keyword",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actor_Names");

            migrationBuilder.DropTable(
                name: "Actor_Video");

            migrationBuilder.DropTable(
                name: "ActorInfo");

            migrationBuilder.DropTable(
                name: "bwh");

            migrationBuilder.DropTable(
                name: "DownHistory");

            migrationBuilder.DropTable(
                name: "FailList_islike_looklater");

            migrationBuilder.DropTable(
                name: "FilesInfo");

            migrationBuilder.DropTable(
                name: "FileToInfo");

            migrationBuilder.DropTable(
                name: "Is_Wm");

            migrationBuilder.DropTable(
                name: "ProducerInfo");

            migrationBuilder.DropTable(
                name: "SearchHistory");

            migrationBuilder.DropTable(
                name: "SpiderLog");

            migrationBuilder.DropTable(
                name: "SpiderTask");

            migrationBuilder.DropTable(
                name: "VideoInfo");
        }
    }
}
