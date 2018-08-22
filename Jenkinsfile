pipeline {
    agent any
    stages {
        stage('Build & Unit Test') {
            steps {
                sh "docker build -t accountownerapp:B${BUILD_NUMBER} -f Dockerfile ."
                sh "docker build -t accountownerapp:test-B${BUILD_NUMBER} -f Dockerfile.Integration ."
            }
        }
        stage('Publish Unit Testing Report'){
            steps {
                script {
                    containerID = sh (
                    script: "docker run -d accountownerapp:B${BUILD_NUMBER}", 
                    returnStdout: true
                    ).trim()
                    echo "Container ID is ==> ${containerID}"
                    sh "docker cp ${containerID}:/TestResults/test_results.trx test_results.trx"
                    sh "docker stop ${containerID}"
                    sh "docker rm ${containerID}"
                    step([$class: 'MSTestPublisher', failOnError: true, testResultsFile: '**/*.trx'])
                }
            }
        }
        stage('Integration Test') {
            steps {
                 sh "docker-compose -f docker-compose.integration.yml up --force-recreate --abort-on-container-exit"
                sh "docker-compose -f docker-compose.integration.yml down -v"
            }
        }
    }
}