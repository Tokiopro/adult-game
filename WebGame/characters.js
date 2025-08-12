// キャラクターデータ
const characters = {
    misaki: {
        id: 'misaki',
        name: '桜井 美咲',
        fullName: '桜井 美咲（さくらい みさき）',
        age: 17,
        grade: '2年生',
        club: 'チアリーダー部',
        personality: 'genki',
        personalityName: '元気系',
        birthday: '4月15日',
        bloodType: 'O型',
        height: '158cm',
        likes: ['カラオケ', 'SNS', 'スイーツ巡り', 'おしゃべり'],
        dislikes: ['勉強', '虫', '暗い場所'],
        favoritePlace: ['屋上', 'カフェテリア', '体育館'],
        color: '#ff6b9d',
        description: 'いつも笑顔で元気いっぱいの女の子。クラスの人気者で、誰とでもすぐに仲良くなれる社交的な性格。チアリーダー部のエースとして活躍中。実は寂しがり屋な一面も。',
        profile: {
            hobby: 'みんなでワイワイするのが大好き！最近はTikTokにハマってるんだ〜',
            dream: '将来はイベントプランナーになりたい！みんなを笑顔にする仕事がしたいの',
            secret: '実は一人でいるのが苦手...だから いつも誰かと一緒にいたくなっちゃう',
            firstImpression: '転校生くん、はじめまして！美咲だよ〜！よろしくね！',
            idealDate: '遊園地でいっぱい遊んで、最後は観覧車で夜景を見たい！'
        },
        routeRequirements: {
            minAffection: 60,
            requiredFlags: ['misaki_confession'],
            conflictCharacters: []
        },
        expressions: {
            normal: '😊',
            happy: '😄',
            sad: '😢',
            angry: '😠',
            surprised: '😲',
            blushing: '😳'
        }
    },
    
    yukino: {
        id: 'yukino',
        name: '藤原 雪乃',
        fullName: '藤原 雪乃（ふじわら ゆきの）',
        age: 18,
        grade: '3年生',
        club: '生徒会（会長）',
        personality: 'kuudere',
        personalityName: 'クール系',
        birthday: '12月24日',
        bloodType: 'A型',
        height: '165cm',
        likes: ['読書', 'クラシック音楽', '紅茶', '静かな場所'],
        dislikes: ['騒がしい場所', '無秩序', '遅刻'],
        favoritePlace: ['図書室', '音楽室', '生徒会室'],
        color: '#66d9ef',
        description: '成績優秀で冷静沈着な生徒会長。クールで近寄りがたい雰囲気があるが、実は優しく面倒見が良い。完璧主義者で、自分にも他人にも厳しいが、心を開いた相手には素直な一面を見せる。',
        profile: {
            hobby: '読書とピアノ。ショパンの曲を弾くと心が落ち着くの',
            dream: '法律家になって、正義を守る仕事がしたいわ',
            secret: '本当は甘いものが大好き。でも、生徒会長のイメージが...',
            firstImpression: '転校生...ね。校則はきちんと守るように。何か困ったことがあれば生徒会室まで',
            idealDate: '美術館でゆっくり絵画鑑賞。その後は静かなカフェでお茶を'
        },
        routeRequirements: {
            minAffection: 70,
            requiredFlags: ['yukino_trust', 'student_council_help'],
            conflictCharacters: []
        },
        expressions: {
            normal: '😐',
            happy: '🙂',
            sad: '😔',
            angry: '😤',
            surprised: '😮',
            blushing: '☺️'
        }
    },
    
    ayame: {
        id: 'ayame',
        name: '小林 あやめ',
        fullName: '小林 あやめ（こばやし あやめ）',
        age: 16,
        grade: '1年生',
        club: '美術部',
        personality: 'dandere',
        personalityName: '内気系',
        birthday: '9月3日',
        bloodType: 'AB型',
        height: '152cm',
        likes: ['絵画', '手芸', '動物', '花'],
        dislikes: ['人混み', '大きな音', '発表'],
        favoritePlace: ['美術室', '中庭', '図書室の隅'],
        color: '#c9b3ff',
        description: '内気で恥ずかしがり屋な後輩。人と話すのが苦手だが、絵を描いている時は別人のように集中する。動物が大好きで、よく中庭で猫と遊んでいる。心を許した相手には、とても献身的。',
        profile: {
            hobby: '絵を描くこと...あと、学校の猫ちゃんたちのお世話も...',
            dream: 'いつか...自分の絵本を出版したいです',
            secret: '先輩のことを描いた絵を...たくさん描いてしまいました////',
            firstImpression: 'あ、あの...せ、先輩...よ、よろしくお願いします...',
            idealDate: '動物園で...いろんな動物を見て...一緒にスケッチしたいです'
        },
        routeRequirements: {
            minAffection: 50,
            requiredFlags: ['ayame_artwork', 'protect_ayame'],
            conflictCharacters: []
        },
        expressions: {
            normal: '😶',
            happy: '😊',
            sad: '😭',
            angry: '😣',
            surprised: '😨',
            blushing: '🥺'
        }
    }
};

// キャラクターイベント定義
const characterEvents = {
    misaki: {
        events: [
            {
                id: 'misaki_intro',
                name: '美咲との出会い',
                requiredAffection: 0,
                description: 'チアリーダー部の練習を見学'
            },
            {
                id: 'misaki_lunch',
                name: 'お昼の誘い',
                requiredAffection: 20,
                description: '美咲と一緒にランチ'
            },
            {
                id: 'misaki_date1',
                name: '初デート',
                requiredAffection: 40,
                description: 'カラオケデート'
            },
            {
                id: 'misaki_trouble',
                name: '美咲の悩み',
                requiredAffection: 60,
                description: '本当の気持ち'
            },
            {
                id: 'misaki_confession',
                name: '告白',
                requiredAffection: 80,
                description: '屋上での告白'
            }
        ]
    },
    
    yukino: {
        events: [
            {
                id: 'yukino_intro',
                name: '生徒会長との出会い',
                requiredAffection: 0,
                description: '生徒会室への呼び出し'
            },
            {
                id: 'yukino_help',
                name: '生徒会の手伝い',
                requiredAffection: 25,
                description: '書類整理を手伝う'
            },
            {
                id: 'yukino_secret',
                name: '雪乃の秘密',
                requiredAffection: 45,
                description: '意外な一面を発見'
            },
            {
                id: 'yukino_trust',
                name: '信頼',
                requiredAffection: 65,
                description: '本音を打ち明けられる'
            },
            {
                id: 'yukino_confession',
                name: '氷が溶ける時',
                requiredAffection: 85,
                description: '音楽室での告白'
            }
        ]
    },
    
    ayame: {
        events: [
            {
                id: 'ayame_intro',
                name: 'あやめとの出会い',
                requiredAffection: 0,
                description: '美術室で出会う'
            },
            {
                id: 'ayame_cats',
                name: '猫と一緒に',
                requiredAffection: 15,
                description: '中庭で猫と遊ぶ'
            },
            {
                id: 'ayame_artwork',
                name: '作品を見せてもらう',
                requiredAffection: 35,
                description: 'あやめの絵を鑑賞'
            },
            {
                id: 'ayame_protect',
                name: '守りたい',
                requiredAffection: 55,
                description: 'あやめを助ける'
            },
            {
                id: 'ayame_confession',
                name: '小さな勇気',
                requiredAffection: 75,
                description: '美術室での告白'
            }
        ]
    }
};

// 好感度による反応の変化
const affectionResponses = {
    low: { // 0-30
        misaki: {
            greeting: 'あ、転校生くん！おはよー！',
            farewell: 'またねー！',
            gift: 'わぁ、ありがとう！',
            compliment: 'えへへ、そんなことないよ〜'
        },
        yukino: {
            greeting: '...おはよう',
            farewell: 'それでは',
            gift: '...受け取っておくわ',
            compliment: '...そう'
        },
        ayame: {
            greeting: 'あ...お、おはようございます...',
            farewell: 'さ、さようなら...',
            gift: 'え...い、いいんですか...？',
            compliment: 'そ、そんな...///'
        }
    },
    
    medium: { // 31-60
        misaki: {
            greeting: 'やっほー！今日も元気？',
            farewell: 'また明日ね！楽しみにしてる！',
            gift: 'わー！美咲のこと考えて選んでくれたの？嬉しい！',
            compliment: 'もう〜、照れちゃうじゃん！'
        },
        yukino: {
            greeting: 'おはよう。今日も一日頑張りましょう',
            farewell: 'また明日。気をつけて帰ってね',
            gift: 'ありがとう。大切にするわ',
            compliment: '...ありがとう。嬉しいわ'
        },
        ayame: {
            greeting: 'せ、先輩...おはようございます',
            farewell: 'また...明日も会えますか...？',
            gift: 'わぁ...すごく嬉しいです...',
            compliment: 'ほ、本当ですか...？ありがとうございます...'
        }
    },
    
    high: { // 61-100
        misaki: {
            greeting: 'まってたよ〜！会いたかった！',
            farewell: '明日まで待てないよ〜...連絡してもいい？',
            gift: 'これ、美咲の好きなやつ！すっごく嬉しい！大好き！',
            compliment: 'そんなこと言われたら...好きになっちゃうじゃん///'
        },
        yukino: {
            greeting: 'おはよう。あなたの顔を見ると...安心するわ',
            farewell: 'もう少し...一緒にいたいけど...また明日ね',
            gift: '私の好みを覚えていてくれたのね...本当に嬉しい',
            compliment: 'あなたにそう言ってもらえると...特別な気持ちになるわ'
        },
        ayame: {
            greeting: 'せ、先輩！待ってました...！',
            farewell: 'もう少しだけ...一緒にいてもいいですか...？',
            gift: 'こんなに素敵なもの...一生大切にします！',
            compliment: '先輩に褒められると...すごくドキドキします...'
        }
    }
};

// ギフトアイテム定義
const giftItems = {
    sweets: {
        name: 'スイーツ',
        description: '美味しそうなケーキ',
        affectionBonus: {
            misaki: 10,
            yukino: 5,
            ayame: 7
        }
    },
    book: {
        name: '本',
        description: '話題の小説',
        affectionBonus: {
            misaki: 3,
            yukino: 10,
            ayame: 7
        }
    },
    artSupplies: {
        name: '画材',
        description: '高級な絵の具セット',
        affectionBonus: {
            misaki: 3,
            yukino: 3,
            ayame: 15
        }
    },
    accessory: {
        name: 'アクセサリー',
        description: 'かわいいヘアピン',
        affectionBonus: {
            misaki: 7,
            yukino: 5,
            ayame: 5
        }
    }
};