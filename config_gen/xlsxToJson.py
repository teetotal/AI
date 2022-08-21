from datetime import datetime
import pandas as pd
import csv
import json
import os

def export():
    excel_file = './config.xlsx'
    all_sheets = pd.read_excel(excel_file, sheet_name=None)
    sheets = all_sheets.keys()

    for sheet_name in sheets:
        if sheet_name[:5].upper() == 'SHEET':
            continue
        
        sheet = pd.read_excel(excel_file, sheet_name=sheet_name)
        sheet.to_csv("./%s.csv" % sheet_name, index=False)

def read(path):
    file = open('./' + path)
    csvreader = csv.reader(file)
    header = next(csvreader)

    return file, csvreader

def write(json_objects, path):
    sz = json.dumps(json_objects, indent=4)
    with open('../config/' + path, 'w') as f:
        f.write(sz)
# Actor ---------------------------------------------------------------------
def get_json_actor(arr):
    j = {
        "enable": True if arr[1].upper() == 'TRUE' else False,
        "village": arr[2],
        "follower": True if arr[3].upper() == 'TRUE' else False,
        "type": int(arr[4]),
        "nickname": arr[5],
        "pets": [], #arr[6]
        "level": int(arr[7]),
        "prefab": arr[8],
        "position": [], #arr[9]
        "rotation": [], #arr[10]
        "trigger": {
            "type": int(arr[11]),
            "value": arr[12]
        },
        "satisfactions": [], #arr[13]
        "inventory": [], #arr[14]
        "isFly": True if arr[15].upper() == 'TRUE' else False,
        "laziness": int(arr[16])
    }

    #pets
    if len(arr[6]) > 0:
        j['pets'] = arr[6].split(',')
    #position
    j['position'] = [float(p) for p in arr[9].split(',')] 
    #rotation
    j['rotation'] = [float(p) for p in arr[10].split(',')] 
    #satisfaction
    satisfactions = arr[13].split('\n')
    for s in satisfactions:
        arr_s = s.split(',')
        satisfaction = {
            "satisfactionId": arr_s[0],
            "min": int(arr_s[1]),
            "max": int(arr_s[2]),
            "value": int(arr_s[3])
        }
        j['satisfactions'].append(satisfaction)
    
    #inventory
    if len(arr[14]) > 0:
        inventories = arr[14].split('\n')
        for i in inventories:
            arr_i = i.split(',')
            inven = {
                "itemId": arr_i[0],
                "quantity": int(arr_i[1]),
                "installation": True if arr_i[2].upper() == 'TRUE' else False
            }
            j['inventory'].append(inven)

    return j, arr[0]

def actors():
    file, csvreader = read('actors.csv')
    json_objects = {}
    for row in csvreader:
        j, actor_id = get_json_actor(row)
        json_objects[actor_id] = j
    file.close()

    write(json_objects, 'actors.json')
# Task ----------------------------------------------------------------------------------
def get_json_task(arr):
    j = {
        "id": arr[0],
        "chain": arr[1],
        "type": int(arr[2]),
        "level": [], #arr[3]
        "village": arr[21],
        "villageLevel": int(arr[4]),
        "title": arr[5],
        "desc": arr[6],
        "animation": arr[7],
        "animationRepeatTime": int(arr[8]),
        "maxRef": int(arr[9]),
        "target": {
            "type": int(arr[10]),
            "value": [], #arr[11]                
            "interaction": {
                "type": int(arr[12]),
                "taskId": arr[13]
            }      
        },
        "satisfactions": {}, #arr[14]
        "satisfactionsRefusal": {}, #arr[15]
        "items": [], #arr[16]
        "materialItems": [], #arr[22]
        "integration": arr[23],
        "scene": arr[17]
    }
    #level
    if len(arr[3]) > 0: 
        level = arr[3].split(',')
        j['level'].append(int(level[0]))
        j['level'].append(int(level[1]))

    #target.value
    j['target']['value'] = arr[11].split('\n')
    #satisfactions
    if len(arr[14]) > 0 :
        satisfactions = arr[14].split(',')
        for s in satisfactions:
            kv = s.split(':')
            if len(kv) != 2:
                continue
            j['satisfactions'][kv[0]] = kv[1]
            
    #satisfactionsRefusal
    if(len(arr[15]) > 0):
        satisfactionsRefusal = arr[15].split(',')
        for s in satisfactionsRefusal:
            kv = s.split(':')
            if len(kv) != 2:
                continue
            j['satisfactionsRefusal'][kv[0]] = kv[1]
            
    #item
    if(len(arr[16]) > 0):
        items = arr[16].split('\n')
        for i in items:
            itemV = i.split(',')
            if len(itemV) != 4:
                continue
            itemId = itemV[0]
            quantity = int(itemV[1])
            winRange = int(itemV[2])
            totalRange = int(itemV[3])
            j['items'].append({
                "itemId": itemId,
                "quantity": quantity,
                "winRange": winRange,
                "totalRange": totalRange
            })
    if(len(arr[22]) > 0):
        items = arr[22].split('\n')
        for i in items:
            itemV = i.split(',')
            if len(itemV) != 2:
                continue
            itemId = itemV[0]
            quantity = int(itemV[1])
            j['materialItems'].append({
                "itemId": itemId,
                "quantity": quantity
            })


    #script
    script = []
    if len(arr[19]) > 0:
        script = arr[19].split('\n')
    
    script_refusal = None
    if len(arr[20]) > 0:
        script_refusal = arr[20].split('\n')

    #json, actor type, id, script, script refusal
    return j, int(arr[18]), arr[0], script, script_refusal

def task():
    file, csvreader = read('task.csv')
  
    json_task = {}
    json_script = {
        "scripts": {},
        "refusal": {}
    }
    for row in csvreader:
        j, actor_type, script_id, script, script_refusal = get_json_task(row)
        if actor_type in json_task:
            json_task[actor_type].append(j)
        else:
            json_task[actor_type] = [j]

        json_script['scripts'][script_id] = script
        if script_refusal != None:
            json_script['refusal'][script_id] = script_refusal
    file.close()

    write(json_task, 'task.json')
    write(json_script, 'script.json')
# Quest --------------------------------------------------------------------------------
def quest():
    def get_json_quest(arr):
        j = {
            "id": arr[0],
            "title": arr[1],
            "desc": arr[2],                
            "values": [], # arr[3]
            "rewards": [] #arr[4]
        }

        #values
        rows = arr[3].split('\n')
        for row in rows:
            if len(row) > 0 :
                kv = row.split(',')
                j["values"].append({
                    "key": kv[0],
                    "value": int(kv[1])
                })

        #rewards
        rows = arr[4].split('\n')
        for row in rows:
            if len(row) > 0 :
                kv = row.split(',')
                j["rewards"].append({
                    "itemId": kv[0],
                    "quantity": int(kv[1])
                })

        actor_type = arr[5]
        return actor_type, j

    file, csvreader = read('quest.csv')
    json_object = {}
    for row in csvreader:
        actor_type, j = get_json_quest(row)
        if actor_type in json_object:
            json_object[actor_type]["quests"].append(j)
        else:
            json_object[actor_type] = {}
            json_object[actor_type]["top"] = 3
            json_object[actor_type]["quests"] = [j]
    file.close()

    write(json_object, 'quest.json')
# Item --------------------------------------------------------------------------------
def item():
    def get_json_item(arr):
        j = {
            "name": arr[1],
            "desc": arr[2],
            "category": int(arr[3]),
            "type": arr[4],
            "level": int(arr[5]),
            "cost": int(arr[6]),        
            "installationKey": arr[7],
            "invoke": {
                "type": int(arr[8]),
                "expire": int(arr[9])
            },
            "satisfaction": [], #arr[10]       
            "draft": [] #arr[17]
        }
        #satisfaction
        if len(arr[10]) > 0:
            s_id = arr[10].split('\n')
            s_min = arr[11].split('\n')
            s_max = arr[12].split('\n')
            s_val = arr[13].split('\n')
            s_m_min = arr[14].split('\n')
            s_m_max = arr[15].split('\n')
            s_m_val = arr[16].split('\n')
            for i in range(len(s_id)):
                s = {
                    "satisfactionId": s_id[i],
                    "min": int(s_min[i]),
                    "max": int(s_max[i]),
                    "value": int(s_val[i]),
                    "measure": {
                        "min": int(s_m_min[i]),
                        "max": int(s_m_max[i]),
                        "value": int(s_m_val[i])
                    }
                }
                j['satisfaction'].append(s)

        #arr[17]
        rows = arr[17].split('\n')
        for row in rows:
            if len(row) == 0: continue
            minMax = row.split(',')
            if len(minMax) == 2:
                j['draft'].append(minMax)

        return arr[0], j

    file, csvreader = read('item.csv')
    json_object = {}
    for row in csvreader:
        item_id, j = get_json_item(row)
        json_object[item_id] = j
    file.close()

    write(json_object, 'item.json')
# Level --------------------------------------------------------------------------------
def level():
    def get_json_level(arr):
        j = {
            "level": arr[1],
            "title": arr[2],
            "next": {
                "threshold": [], #arr[3]
                "rewards": [] #arr[4]
            }
        }
        #threshold
        if len(arr[3]) > 0:
            threshold = arr[3].split('\n')
            for p in threshold:
                kv = p.split(',')
                j['next']['threshold'].append({
                    "key": kv[0],
                    "value": int(kv[1])
                })
        
        if len(arr[4]) > 0:
            reward = arr[4].split('\n')
            for p in reward:
                kv = p.split(',')
                j['next']['rewards'].append({
                    "itemId": kv[0],
                    "quantity": int(kv[1])
                })
        return arr[0], j

    file, csvreader = read('level.csv')
    json_object = {}
    for row in csvreader:
        actor_type, j = get_json_level(row)
        if actor_type in json_object:
            json_object[actor_type]['levels'].append(j)
        else:
            json_object[actor_type] = {
                "startLevel": 1,   
                "levels": [j]
            }
    file.close()

    write(json_object, 'level.json')
# Vehicle --------------------------------------------------------------------------------
def vehicle():
    def get_json_vehicle(arr):
        j = {
            "type": arr[0],
            "vehicleId": arr[1],
            "ownable": True if arr[2].upper() == 'TRUE' else False,
            "name": arr[3],
            "speed": float(arr[4]),
            "acceleration": float(arr[5]),
            "waiting": int(arr[6]),
            "prefab": arr[7],
            "owner": arr[8],
            "village": arr[9],
            "positions": [] #arr[10]
        }
        #positions
        if len(arr[10]) > 0:
            positions = arr[10].split('\n')
            for p in positions:
                pos_rot = p.split(':')
                j['positions'].append({
                    "position": pos_rot[0],
                    "rotation": pos_rot[1]
                })

        return j

    file, csvreader = read('vehicle.csv')
    json_object = {}
    for row in csvreader:
        j = get_json_vehicle(row)
        json_object[j["vehicleId"]] = j
    file.close()

    write(json_object, 'vehicle.json')
# Satisfaction ------------------------------------------------
def satisfaction():
    def get_json_satisfaction(arr):
        j = {
            "satisfactionId": arr[0],
            "title": arr[1],
            "type": arr[2],
            "discharge": float(arr[3]),
            "period": arr[4]
        }
        return j, arr[0]

    file, csvreader = read('satisfaction.csv')
    json_object = {}
    for row in csvreader:
        j, id = get_json_satisfaction(row)
        json_object[id] = j
    file.close()

    write(json_object, 'satisfactions.json')
#--------------------------------------------------------------
export()
print(datetime.now(), 'exported')
level()
print(datetime.now(), 'gen level')
item()
print(datetime.now(), 'gen item')
actors()
print(datetime.now(), 'gen actors')
task()
print(datetime.now(), 'gen task')
quest()
print(datetime.now(), 'gen quest')
vehicle()
print(datetime.now(), 'gen vehicle')
satisfaction()
print(datetime.now(), 'gen satisfaction')
os.remove('./task.csv')
os.remove('./actors.csv')
os.remove('./quest.csv')
os.remove('./item.csv')
os.remove('./level.csv')
os.remove('./vehicle.csv')
os.remove('./satisfaction.csv')
print(datetime.now(), 'removed csv files')