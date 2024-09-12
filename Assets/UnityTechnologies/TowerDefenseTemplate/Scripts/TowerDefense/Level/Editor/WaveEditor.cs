using System.Collections.Generic;
using System.Linq;
using TowerDefense.Agents.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TowerDefense.Level.Editor
{
	/// <summary>
	/// Custom editor to display wave time sum
	/// </summary>
	[CustomEditor(typeof(Wave), true)]
	public class WaveEditor : UnityEditor.Editor
	{
		Wave m_Wave;

		ReorderableList wavesList;

		static GUIContent TempContent = new GUIContent();

		void OnEnable()
		{
			m_Wave = (Wave) target;

			var instructionsListProp = serializedObject.FindProperty("spawnInstructions");
			wavesList = new ReorderableList(serializedObject, instructionsListProp);

			var elementHeight = 18;
			wavesList.drawHeaderCallback += (Rect rect) => 
			{
				TempContent.text = "Spawn Instructions";
				EditorGUI.PrefixLabel(rect, TempContent);
			};
			wavesList.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				if (index >= instructionsListProp.arraySize)
					return;
				var prop = instructionsListProp.GetArrayElementAtIndex(index);
				var data = m_Wave.spawnInstructions[index];
				var labelWidth = EditorGUIUtility.labelWidth;

				var itemWidth = rect.width / 3 - 15;
				rect.height = elementHeight;
				//rect.width = 20;
				//rect.x += 20;
				//EditorGUIUtility.labelWidth = 20;
				//TempContent.text = index.ToString();
				//GUI.Label(rect, TempContent);
				EditorGUIUtility.labelWidth = 20;
				TempContent.text = index.ToString();
				rect.width = itemWidth;
				EditorGUI.PropertyField(rect, prop.FindPropertyRelative("agentConfiguration"), TempContent);
				//data.agentConfiguration = (AgentConfiguration)EditorGUI.ObjectField(rect, prop.FindPropertyRelative("agentConfiguration"), typeof(AgentConfiguration), false);
				rect.x += itemWidth;
				EditorGUI.PropertyField(rect, prop.FindPropertyRelative("startingNode"), GUIContent.none);
				//data.startingNode = (Nodes.Node)EditorGUI.ObjectField(rect, data.startingNode, typeof(Nodes.Node), true);
				rect.x += itemWidth;

				EditorGUIUtility.labelWidth = 40;
				TempContent.text = "Delay";
				EditorGUI.PropertyField(rect, prop.FindPropertyRelative("delayToSpawn"), TempContent);
				rect.x += itemWidth;
				//data.delayToSpawn = EditorGUI.FloatField(rect, "Delay", data.delayToSpawn);
				
				EditorGUIUtility.labelWidth = 25;
				rect.width = 25;

				if (GUI.Button(rect, "+"))
				{
					instructionsListProp.InsertArrayElementAtIndex(index);
				}
				rect.x += 25;
				if (GUI.Button(rect, "-"))
				{
					instructionsListProp.DeleteArrayElementAtIndex(index);
				}
				EditorGUIUtility.labelWidth = labelWidth;
			};
			wavesList.elementHeight = elementHeight;
		}

		public override void OnInspectorGUI()
		{
			//base.OnInspectorGUI();

			// Draw a summary of all spawn instructions
			List<SpawnInstruction> spawnInstructions = m_Wave.spawnInstructions;
			if (spawnInstructions == null)
			{
				return;
			}

			serializedObject.Update();
			if (m_Wave is TimedWave)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToNextWave"));
			}
			
			wavesList.DoLayoutList();

			serializedObject.ApplyModifiedProperties();
			
			// Count spawn instructions
			float lastSpawnTime = spawnInstructions.Sum(t => t.delayToSpawn);

			// Group by enemy type so we can count per type as well
			var groups = spawnInstructions.GroupBy(t => t.agentConfiguration);
			var groupCounts = groups.Select(g => new {Number = g.Count(), Item = g.Key.agentName});

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Wave summary");

			EditorGUILayout.LabelField(string.Format("Last spawn time: {0}", lastSpawnTime));
			EditorGUILayout.Space();
			foreach (var groupCount in groupCounts)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(string.Format("Enemy:\t{0}", groupCount.Item));
				EditorGUILayout.LabelField(string.Format("Count:\t{0}", groupCount.Number));
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}