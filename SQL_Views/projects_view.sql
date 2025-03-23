create or replace definer = * `%` view projects_view as
       
SELECT
    p.id AS PROJECT_ID,
    p.name AS PROJECT_NAME,

    -- Убираем переносы строк и HTML-теги
    TRIM(
            REPLACE(
                    REPLACE(
                            REPLACE(
                                    REPLACE(
                                            REPLACE(
                                                    REGEXP_REPLACE(p.description, '<[^>]+>', ''),
                                                    '&quot;', ''
                                            ),
                                            '&nbsp;', ' '
                                    ),
                                    '&laquo;', ''
                            ),
                            '&raquo;', ''
                    ),
                    '\n', ''
            )
    ) AS PROJECT_DESCRIPTION,

    -- JSON-массив трекеров (фикс: JSON_ARRAY вместо одиночного JSON_OBJECT)
    (
        SELECT JSON_ARRAYAGG(
                       JSON_OBJECT(
                               'trackerId', t.id,
                               'trackerName', t.name
                       )
               )
        FROM trackers t
                 JOIN projects_trackers pt ON t.id = pt.tracker_id
        WHERE pt.project_id = p.id
    ) AS TRACKERS_JSON,

    -- JSON-массив групп и их ролей
    (
        SELECT JSON_ARRAYAGG(
                       JSON_OBJECT(
                               'groupId', g.id,
                               'groupName', CONCAT(g.firstname, ' ', g.lastname),
                               'roles', COALESCE(
                                       (
                                           SELECT JSON_ARRAYAGG(
                                                          JSON_OBJECT(
                                                                  'roleId', r2.id,
                                                                  'roleName', r2.name
                                                          )
                                                  )
                                           FROM member_roles mr2
                                                    JOIN roles r2 ON r2.id = mr2.role_id
                                           WHERE mr2.member_id = mg.id
                                       ),
                                       JSON_ARRAY() -- Пустой массив, если нет ролей
                                        )
                       )
               )
        FROM users g
                 JOIN members mg ON mg.user_id = g.id
        WHERE g.type = 'Group'
          AND mg.project_id = p.id
    ) AS GROUPS_JSON

FROM projects p
WHERE p.status = 1
ORDER BY p.id;

