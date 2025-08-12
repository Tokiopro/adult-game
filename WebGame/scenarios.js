// シナリオデータ
const scenarios = {
    // プロローグ
    prologue: {
        id: 'prologue',
        name: 'プロローグ',
        dialogues: [
            {
                speaker: '',
                text: '春の暖かい日差しが降り注ぐ4月。',
                background: 'classroom'
            },
            {
                speaker: '',
                text: '僕は転校生として、この学園にやってきた。'
            },
            {
                speaker: '担任',
                text: 'えー、今日から転校生が来ています。仲良くしてあげてください。'
            },
            {
                speaker: '主人公',
                text: 'はじめまして、よろしくお願いします。',
                choices: [
                    {
                        text: '元気よく自己紹介する',
                        affection: { misaki: 5 },
                        next: 1
                    },
                    {
                        text: '落ち着いて自己紹介する',
                        affection: { yukino: 5 },
                        next: 1
                    },
                    {
                        text: '控えめに自己紹介する',
                        affection: { ayame: 5 },
                        next: 1
                    }
                ]
            },
            {
                speaker: '',
                text: 'クラスメイトたちが興味深そうにこちらを見ている。',
                effect: 'flash'
            },
            {
                speaker: '???',
                text: 'やっほー！転校生くん！',
                character: 'misaki',
                expression: 'happy'
            },
            {
                speaker: '美咲',
                text: 'わたし桜井美咲！チアリーダー部なの！よろしくね〜！',
                character: 'misaki',
                expression: 'happy'
            },
            {
                speaker: '主人公',
                text: 'よろしく、美咲さん。'
            },
            {
                speaker: '美咲',
                text: 'もー！美咲でいいよ！堅苦しいのは苦手なんだから〜',
                character: 'misaki',
                expression: 'normal'
            },
            {
                speaker: '',
                text: 'その時、凛とした雰囲気の女生徒が近づいてきた。',
                character: null
            },
            {
                speaker: '???',
                text: '騒がしいわね、美咲さん。',
                character: 'yukino',
                expression: 'normal'
            },
            {
                speaker: '雪乃',
                text: '私は藤原雪乃。生徒会長をしています。',
                character: 'yukino',
                expression: 'normal'
            },
            {
                speaker: '雪乃',
                text: '何か困ったことがあれば、生徒会室まで来てください。',
                character: 'yukino',
                expression: 'normal'
            },
            {
                speaker: '主人公',
                text: 'ありがとうございます、藤原先輩。'
            },
            {
                speaker: '',
                text: '放課後になった...',
                background: 'courtyard'
            },
            {
                speaker: '',
                text: '中庭を歩いていると、一人の女の子が猫と遊んでいるのを見つけた。'
            },
            {
                speaker: '???',
                text: 'あ...！',
                character: 'ayame',
                expression: 'surprised'
            },
            {
                speaker: 'あやめ',
                text: 'す、すみません...邪魔でしたか...？',
                character: 'ayame',
                expression: 'normal'
            },
            {
                speaker: '主人公',
                text: 'いや、全然。猫が好きなの？'
            },
            {
                speaker: 'あやめ',
                text: 'は、はい...動物が好きで...わたし、小林あやめです...',
                character: 'ayame',
                expression: 'blushing'
            },
            {
                speaker: '',
                text: 'こうして、新しい学園生活が始まった。',
                character: null
            }
        ]
    },

    // Day 1 朝のイベント
    day1_morning: {
        id: 'day1_morning',
        name: '1日目の朝',
        dialogues: [
            {
                speaker: '',
                text: '転校して2日目の朝。',
                background: 'classroom'
            },
            {
                speaker: '',
                text: '教室に入ると、すでに何人かの生徒が登校していた。'
            },
            {
                speaker: '',
                text: '誰に話しかけようか...',
                choices: [
                    {
                        text: '美咲に話しかける',
                        affection: { misaki: 10 },
                        flag: 'talked_misaki_day1',
                        next: 2
                    },
                    {
                        text: '雪乃に話しかける',
                        affection: { yukino: 10 },
                        flag: 'talked_yukino_day1',
                        next: 5
                    },
                    {
                        text: 'あやめを探す',
                        affection: { ayame: 10 },
                        flag: 'talked_ayame_day1',
                        next: 8
                    }
                ]
            },
            // 美咲ルート
            {
                speaker: '美咲',
                text: 'あ！転校生くん、おはよー！',
                character: 'misaki',
                expression: 'happy'
            },
            {
                speaker: '美咲',
                text: '今日も元気？朝から会えて嬉しいな〜',
                character: 'misaki',
                expression: 'happy',
                next: 11
            },
            // 雪乃ルート
            {
                speaker: '雪乃',
                text: 'おはよう。早いのね。',
                character: 'yukino',
                expression: 'normal'
            },
            {
                speaker: '雪乃',
                text: '規則正しい生活は大切よ。感心するわ。',
                character: 'yukino',
                expression: 'normal'
            },
            {
                speaker: '',
                text: '雪乃は少し微笑んだような気がした。',
                next: 11
            },
            // あやめルート
            {
                speaker: '',
                text: '美術室であやめを見つけた。',
                background: 'classroom'
            },
            {
                speaker: 'あやめ',
                text: 'せ、先輩...おはようございます...',
                character: 'ayame',
                expression: 'blushing'
            },
            {
                speaker: 'あやめ',
                text: '朝は...静かで好きです...',
                character: 'ayame',
                expression: 'normal'
            },
            // 合流
            {
                speaker: '',
                text: '充実した朝の時間を過ごした。'
            }
        ]
    },

    // 日常イベント
    daily_event: {
        id: 'daily_event',
        name: '日常',
        dialogues: [
            {
                speaker: '',
                text: '今日も一日が始まる。',
                background: 'classroom'
            },
            {
                speaker: '',
                text: '何をして過ごそうか...',
                choices: [
                    {
                        text: '屋上でお昼を食べる',
                        flag: 'lunch_rooftop',
                        next: 2
                    },
                    {
                        text: '図書室で勉強する',
                        flag: 'study_library',
                        next: 4
                    },
                    {
                        text: '部活動を見学する',
                        flag: 'club_visit',
                        next: 6
                    }
                ]
            },
            // 屋上
            {
                speaker: '',
                text: '屋上は気持ちいい風が吹いていた。',
                background: 'rooftop'
            },
            {
                speaker: '',
                text: 'のんびりとした時間を過ごした。',
                next: 8
            },
            // 図書室
            {
                speaker: '',
                text: '図書室は静かで集中できる。',
                background: 'library'
            },
            {
                speaker: '',
                text: '勉強がはかどった。',
                next: 8
            },
            // 部活
            {
                speaker: '',
                text: 'いろいろな部活を見て回った。',
                background: 'courtyard'
            },
            {
                speaker: '',
                text: 'みんな一生懸命で刺激を受けた。'
            },
            // 終了
            {
                speaker: '',
                text: '今日も一日が終わった。'
            }
        ]
    },

    // 美咲イベント1
    misaki_event1: {
        id: 'misaki_event1',
        name: '美咲とランチ',
        dialogues: [
            {
                speaker: '',
                text: 'お昼休み、美咲が声をかけてきた。',
                background: 'classroom'
            },
            {
                speaker: '美咲',
                text: 'ねぇねぇ！一緒にお昼食べよ！',
                character: 'misaki',
                expression: 'happy'
            },
            {
                speaker: '美咲',
                text: 'カフェテリアの新メニューが気になってるんだ〜',
                character: 'misaki',
                expression: 'normal'
            },
            {
                speaker: '',
                text: '美咲と一緒にカフェテリアへ向かった。',
                background: 'cafeteria'
            },
            {
                speaker: '美咲',
                text: 'わぁ！美味しそう〜！',
                character: 'misaki',
                expression: 'happy'
            },
            {
                speaker: '美咲',
                text: 'ねぇ、転校生くんってどんなこと好きなの？',
                character: 'misaki',
                expression: 'normal',
                choices: [
                    {
                        text: 'スポーツが好き',
                        affection: { misaki: 10 },
                        next: 6
                    },
                    {
                        text: '読書が好き',
                        affection: { misaki: 5 },
                        next: 8
                    },
                    {
                        text: '美咲といるのが好き',
                        affection: { misaki: 15 },
                        next: 10
                    }
                ]
            },
            // スポーツ
            {
                speaker: '美咲',
                text: 'やっぱり！運動してる人って素敵だよね！',
                character: 'misaki',
                expression: 'happy'
            },
            {
                speaker: '美咲',
                text: '今度チアの練習見に来てよ！',
                next: 12
            },
            // 読書
            {
                speaker: '美咲',
                text: 'へぇ〜、インテリなんだ！',
                character: 'misaki',
                expression: 'normal'
            },
            {
                speaker: '美咲',
                text: '美咲も本読んでみようかな〜',
                next: 12
            },
            // 美咲といる
            {
                speaker: '美咲',
                text: 'えっ...！？',
                character: 'misaki',
                expression: 'blushing'
            },
            {
                speaker: '美咲',
                text: 'も、もう〜！からかわないでよ〜！////',
                character: 'misaki',
                expression: 'blushing'
            },
            // 終了
            {
                speaker: '',
                text: '楽しいランチタイムを過ごした。'
            }
        ]
    },

    // 雪乃イベント1
    yukino_event1: {
        id: 'yukino_event1',
        name: '生徒会の手伝い',
        dialogues: [
            {
                speaker: '',
                text: '放課後、生徒会室を訪れた。',
                background: 'classroom'
            },
            {
                speaker: '雪乃',
                text: 'あら、来てくれたのね。',
                character: 'yukino',
                expression: 'normal'
            },
            {
                speaker: '雪乃',
                text: '実は書類整理を手伝ってもらいたくて...',
                character: 'yukino'
            },
            {
                speaker: '',
                text: '雪乃と一緒に書類を整理することになった。'
            },
            {
                speaker: '雪乃',
                text: '...仕事が早いのね。助かるわ。',
                character: 'yukino',
                expression: 'happy'
            },
            {
                speaker: '',
                text: '作業中、雪乃の横顔を見ていると...',
                choices: [
                    {
                        text: '綺麗だなと思う',
                        affection: { yukino: 10 },
                        next: 6
                    },
                    {
                        text: '真面目だなと思う',
                        affection: { yukino: 5 },
                        next: 8
                    },
                    {
                        text: 'もっと知りたいと思う',
                        affection: { yukino: 15 },
                        next: 10
                    }
                ]
            },
            // 綺麗
            {
                speaker: '雪乃',
                text: '...何か？',
                character: 'yukino',
                expression: 'normal'
            },
            {
                speaker: '雪乃',
                text: '見つめられると...集中できないわ。',
                character: 'yukino',
                expression: 'blushing',
                next: 12
            },
            // 真面目
            {
                speaker: '雪乃',
                text: '当然よ。生徒会長として当然のことをしているだけ。',
                character: 'yukino',
                expression: 'normal'
            },
            {
                speaker: '',
                text: 'でも、少し嬉しそうだった。',
                next: 12
            },
            // もっと知りたい
            {
                speaker: '雪乃',
                text: '私のことを...？',
                character: 'yukino',
                expression: 'surprised'
            },
            {
                speaker: '雪乃',
                text: '...変わった人ね。でも、嫌いじゃないわ。',
                character: 'yukino',
                expression: 'blushing'
            },
            // 終了
            {
                speaker: '',
                text: '充実した時間を過ごせた。'
            }
        ]
    },

    // あやめイベント1
    ayame_event1: {
        id: 'ayame_event1',
        name: 'あやめの絵',
        dialogues: [
            {
                speaker: '',
                text: '美術室であやめを見つけた。',
                background: 'classroom'
            },
            {
                speaker: 'あやめ',
                text: 'あ...先輩...',
                character: 'ayame',
                expression: 'surprised'
            },
            {
                speaker: 'あやめ',
                text: '絵を...描いてました...',
                character: 'ayame',
                expression: 'normal'
            },
            {
                speaker: '',
                text: 'あやめの絵を見せてもらった。'
            },
            {
                speaker: '',
                text: 'とても美しい風景画だった。',
                choices: [
                    {
                        text: 'すごく上手だね',
                        affection: { ayame: 10 },
                        next: 5
                    },
                    {
                        text: '心が込もってる',
                        affection: { ayame: 15 },
                        next: 7
                    },
                    {
                        text: '今度モデルになるよ',
                        affection: { ayame: 20 },
                        next: 9
                    }
                ]
            },
            // 上手
            {
                speaker: 'あやめ',
                text: 'あ、ありがとうございます...',
                character: 'ayame',
                expression: 'blushing'
            },
            {
                speaker: 'あやめ',
                text: 'でも...まだまだです...',
                next: 11
            },
            // 心が込もってる
            {
                speaker: 'あやめ',
                text: '！...そんな...',
                character: 'ayame',
                expression: 'surprised'
            },
            {
                speaker: 'あやめ',
                text: '先輩に...わかってもらえて...嬉しいです...',
                character: 'ayame',
                expression: 'happy',
                next: 11
            },
            // モデル
            {
                speaker: 'あやめ',
                text: 'ほ、本当ですか...！？',
                character: 'ayame',
                expression: 'surprised'
            },
            {
                speaker: 'あやめ',
                text: 'せ、先輩を描けるなんて...夢みたい...',
                character: 'ayame',
                expression: 'blushing'
            },
            // 終了
            {
                speaker: '',
                text: 'あやめと素敵な時間を過ごした。'
            }
        ]
    },

    // エンディング用シナリオ（サンプル）
    ending_misaki: {
        id: 'ending_misaki',
        name: '美咲エンディング',
        dialogues: [
            {
                speaker: '',
                text: '屋上で美咲が待っていた。',
                background: 'rooftop'
            },
            {
                speaker: '美咲',
                text: '来てくれた...！',
                character: 'misaki',
                expression: 'blushing'
            },
            {
                speaker: '美咲',
                text: 'あのね...ずっと言いたかったことがあるの...',
                character: 'misaki',
                expression: 'blushing'
            },
            {
                speaker: '美咲',
                text: '美咲...転校生くんのこと...大好き！',
                character: 'misaki',
                expression: 'happy'
            },
            {
                speaker: '',
                text: '～美咲 Happy End～'
            }
        ]
    }
};