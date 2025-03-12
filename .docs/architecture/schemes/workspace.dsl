workspace "Name" "Description" {

    !identifiers hierarchical
    !docs docs
    !adrs adr

    model {
        prometheus = element "Prometheus" {
            tags "Prometheus"
        }        
        grafana = element "Grafana" {
            tags "Grafana"
        }
        jaeger = element "Jaeger" {
            tags "Jaeger"
        }
        ss = softwareSystem "Prototype" {
            host = container "Host" {
                act_main = component "Main:Entry Activity" {
                    tags "Main_Entry_Activity"
                }
                act_lib1_entry = component "Lib1:Entry Activity" {
                    tags "Lib1_Entry_Activity"
                }
            }
            act_main = container "Main:Entry Activity" {
                tags "Container_Main_Entry_Activity"
            }
            act_lib1_entry = container "Lib1:Entry Activity" {
                tags "Container_Lib1_Entry_Activity"
            }
            act_lib1_a = container "Lib1:A Activity" {
                tags "Container_Lib1_A_Activity"
            }
            act_lib1_b = container "Lib1:B Activity" {
                tags "Container_Lib1_B_Activity"
            }
            act_lib1_c = container "Lib1:C Activity" {
                tags "Container_Lib1_C_Activity"
            }
            act_lib1_d = container "Lib1:D Activity" {
                tags "Container_Lib1_D_Activity"
            }
            act_lib2_entry = container "Lib2:Entry Activity" {
                tags "Container_Lib2_Entry_Activity"
            }
            act_lib2_a = container "Lib2:A Activity" {
                tags "Container_Lib2_A_Activity"
            }
            act_lib2_b = container "Lib2:B Activity" {
                tags "Container_Lib2_B_Activity"
            }
        }

        ss.host -> ss.act_main "Запускает по графику"

        ss.act_main -> ss.act_lib1_entry "Запускает (ставит в очередь)"
        ss.act_lib1_entry -> ss.act_lib1_a "Запускает (ставит в очередь)"
        ss.act_lib1_a -> ss.act_lib1_b "Запускает (ставит в очередь)"
        ss.act_lib1_b -> ss.act_lib1_c "Запускает (ставит в очередь)"
        ss.act_lib1_c -> ss.act_lib1_d "Запускает (ставит в очередь)"
        
        ss.act_main -> ss.act_lib2_entry "Запускает (ставит в очередь)"
        ss.act_lib2_entry -> ss.act_lib2_a "Запускает (ставит в очередь)"
        ss.act_lib2_a -> ss.act_lib2_b "Запускает (ставит в очередь)"

        ss.act_main -> prometheus "Отправляет метрики"
        ss.act_lib1_entry -> prometheus "Отправляет метрики"
        ss.act_lib1_a -> prometheus "Отправляет метрики"
        ss.act_lib1_b -> prometheus "Отправляет метрики"
        ss.act_lib1_c -> prometheus "Отправляет метрики"
        ss.act_lib2_entry -> prometheus "Отправляет метрики"
        ss.act_lib2_a -> prometheus "Отправляет метрики"
        ss.act_lib2_b -> prometheus "Отправляет метрики"

        ss.act_main -> jaeger "Отправляет трейсы"
        ss.act_lib1_entry -> jaeger "Отправляет трейсы"
        ss.act_lib1_a -> jaeger "Отправляет трейсы"
        ss.act_lib1_b -> jaeger "Отправляет трейсы"
        ss.act_lib1_c -> jaeger "Отправляет трейсы"
        ss.act_lib2_entry -> jaeger "Отправляет трейсы"
        ss.act_lib2_a -> jaeger "Отправляет трейсы"
        ss.act_lib2_b -> jaeger "Отправляет трейсы"

        grafana -> prometheus "Отображает метрики"
        grafana -> jaeger "Отображает трейсы"
        
        ss.host.act_main -> ss.host.act_lib1_entry "Ставит в очередь"
    }

    views {
        container ss "System" {
            include *
            include grafana
            autolayout lr
        }

        component ss.host "Host" {
            include *
            autolayout lr
        }

        styles {
            element "Element" {
                color #ffffff
            }
            element "Person" {
                background #d34407
                shape person
            }
            element "Software System" {
                background #f86628
            }
            element "Container" {
                background #f88728
            }
            element "Database" {
                shape cylinder
            }
        }
    }

    configuration {
        scope softwaresystem
    }

}