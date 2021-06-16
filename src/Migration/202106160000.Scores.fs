namespace Migrations

open SimpleMigrations

[<Migration(202106160000L, "Create Scores")>]
type CreateScore() =
    inherit Migration()

    override __.Up() =
        base.Execute(
            """
                CREATE TABLE scores(
                	id UUID NOT NULL PRIMARY KEY,
                	cpf BIGINT NOT NULL,
                	value INT NOT NULL,
                	created_at TIMESTAMPTZ NOT NULL
                );

                CREATE INDEX scores_find_by_cpf ON scores(
                    cpf ASC,
                    created_at DESC
                );
            """
        )

    override __.Down() = base.Execute("DROP TABLE scores")
