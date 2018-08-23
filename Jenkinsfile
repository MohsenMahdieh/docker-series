pipeline {
    agent any
    options {
        timeout(time: 1, unit: 'HOURS') 
        timestamps()
    }
    stages {
        stage('Build & Unit Test') {
            steps {
                slackSend color: "good", message : "Build Started - ${env.JOB_NAME} ${env.BUILD_NUMBER} (<${env.BUILD_URL}|Open>)"
                sh "docker build -t accountownerapp:B${BUILD_NUMBER} -f Dockerfile ."
                sh "docker build -t accountownerapp:test-B${BUILD_NUMBER} -f Dockerfile.Integration ."
            }
        }
        stage('Publish Unit Testing & Code Coverage Reports'){
            steps {
                script {
                    containerID = sh (
                    script: "docker run -d accountownerapp:B${BUILD_NUMBER}", 
                    returnStdout: true
                    ).trim()
                    echo "Container ID is ==> ${containerID}"
                    sh "docker cp ${containerID}:/TestResults/test_results.trx test_results.trx"
                    sh "docker cp ${containerID}:/TestResults/coverage.xml coverage.xml"
                    sh "docker stop ${containerID}"
                    sh "docker rm ${containerID}"
                    step([$class: 'MSTestPublisher', failOnError: true, testResultsFile: '**/*.trx'])
                    step([$class: 'CoberturaPublisher', autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/coverage.xml', failUnhealthy: false, failUnstable: false, maxNumberOfBuilds: 0, onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false])
                }
            }
        }
        stage('Integration Test') {
            steps {
                sh "docker-compose -f docker-compose.integration.yml up --force-recreate --abort-on-container-exit"
                sh "docker-compose -f docker-compose.integration.yml down -v"
            }
        }
        stage('Pushing Conatiners to Registry') {
            environment {
                ACR_CREDS = credentials('acr-credential')
            }
            steps {
                WEB_IMAGE_NAME="${env.ACR_LOGINSERVER}/accountownerapp:B${BUILD_NUMBER}"
                sh "docker login ${env.ACR_LOGINSERVER} -u ${ACR_CREDS_USR} -p ${ACR_CREDS_PSW}"
                sh "docker push $WEB_IMAGE_NAME"
            }
        }
        // stage('Publish Integration Testing Report'){
        //     steps {
        //         script {
        //             containerID = sh (
        //             script: "docker run -d accountownerapp:B${BUILD_NUMBER}", 
        //             returnStdout: true
        //             ).trim()
        //             echo "Container ID is ==> ${containerID}"
        //             sh "docker cp ${containerID}:/TestResults/coverage.trx coverage.trx"
        //             sh "docker stop ${containerID}"
        //             sh "docker rm ${containerID}"
        //             step([$class: 'MSTestPublisher', failOnError: true, testResultsFile: '**/*.xml'])
        //         }
        //     }
        // }
    }
    post {
        always {
            
        }
        success {
            slackSend color: "good", message : "Build finished sucessfully - ${env.JOB_NAME} ${env.BUILD_NUMBER} (<${env.BUILD_URL}|Open>)"
        }
        failure {
            slackSend color: "#439FE0", message : "Build failed - ${env.JOB_NAME} ${env.BUILD_NUMBER} (<${env.BUILD_URL}|Open>)"
        }
    }
}